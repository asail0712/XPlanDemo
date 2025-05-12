using System;
using System.IO;
using UnityEngine;

namespace XPlan.Audio
{
    public static class AudioTools
    {
        public enum PCMFormatSize
        {
            EightBit    = 8,
            SixteenBit  = 16
        }

        public static byte[] EncodeToWav(this AudioClip audioClip, PCMFormatSize bitDepth = PCMFormatSize.SixteenBit, bool trim = false, int outputSampleRate = 44100)
        {
            if (audioClip == null)
            {
                Debug.LogError("AudioClip is null.");
                return null;
            }

            int channels    = audioClip.channels;
            int sampleRate  = audioClip.frequency;
            int samples     = audioClip.samples;

            float[] data = new float[samples * channels];
            audioClip.GetData(data, 0);

            if (trim)
            {
                data    = TrimSilence(data, channels);
            }

            if (sampleRate != outputSampleRate)
            {
                data        = Resample(data, sampleRate, outputSampleRate, channels);
                sampleRate  = outputSampleRate;
            }

            byte[] wavData  = ConvertToWav(data, sampleRate, channels, bitDepth);
            return wavData;
        }


        public static AudioClip DecodeToClip(
            byte[] pcmData,
            PCMFormatSize size = PCMFormatSize.SixteenBit,
            int inputSampleRate = 44100,
            int outputSampleRate = 44100,
            int channels = 1)
        {
            if (pcmData == null || pcmData.Length == 0)
            {
                Debug.LogError("PCM data is null or empty.");
                return null;
            }

            float[] samples;

            switch (size)
            {
                case PCMFormatSize.SixteenBit:
                    int sampleCount16 = pcmData.Length / 2;
                    samples = new float[sampleCount16];
                    for (int i = 0; i < sampleCount16; i++)
                    {
                        short sample = BitConverter.ToInt16(pcmData, i * 2);
                        samples[i] = sample / 32768f;
                    }
                    break;

                case PCMFormatSize.EightBit:
                    int sampleCount8 = pcmData.Length;
                    samples = new float[sampleCount8];
                    for (int i = 0; i < sampleCount8; i++)
                    {
                        samples[i] = (pcmData[i] - 128) / 128f;
                    }
                    break;

                default:
                    Debug.LogError("Unsupported PCM format size.");
                    return null;
            }

            // 若需要，進行重取樣
            float[] outputSamples;
            if (inputSampleRate != outputSampleRate)
            {
                outputSamples = Resample(samples, inputSampleRate, outputSampleRate, channels);
            }
            else
            {
                outputSamples = samples;
            }

            int totalSamples        = outputSamples.Length;
            int samplesPerChannel   = totalSamples / channels;

            AudioClip audioClip = AudioClip.Create("DecodedClip", samplesPerChannel, channels, outputSampleRate, false);
            audioClip.SetData(outputSamples, 0);

            return audioClip;
        }

        private static float[] TrimSilence(float[] data, int channels, float threshold = 0.01f)
        {
            int start   = 0;
            int end     = data.Length - 1;

            // Find start index
            for (int i = 0; i < data.Length; i += channels)
            {
                bool silent = true;
                for (int c = 0; c < channels; c++)
                {
                    if (Mathf.Abs(data[i + c]) > threshold)
                    {
                        silent = false;
                        break;
                    }
                }
                if (!silent)
                {
                    start = i;
                    break;
                }
            }

            // Find end index
            for (int i = data.Length - channels; i >= 0; i -= channels)
            {
                bool silent = true;
                for (int c = 0; c < channels; c++)
                {
                    if (Mathf.Abs(data[i + c]) > threshold)
                    {
                        silent = false;
                        break;
                    }
                }
                if (!silent)
                {
                    end = i + channels - 1;
                    break;
                }
            }

            int length = end - start + 1;
            if (length <= 0) return new float[0];

            float[] trimmedData = new float[length];
            Array.Copy(data, start, trimmedData, 0, length);
            return trimmedData;
        }

        private static float[] Resample(float[] data, int originalRate, int targetRate, int channels)
        {
            int originalLength  = data.Length / channels;
            int newLength       = Mathf.FloorToInt((float)originalLength * targetRate / originalRate);
            float[] newData     = new float[newLength * channels];

            for (int i = 0; i < newLength; i++)
            {
                float t     = (float)i * originalRate / targetRate;
                int index   = Mathf.FloorToInt(t);
                float frac  = t - index;

                for (int c = 0; c < channels; c++)
                {
                    float sample1               = data[Mathf.Clamp(index * channels + c, 0, data.Length - 1)];
                    float sample2               = data[Mathf.Clamp((index + 1) * channels + c, 0, data.Length - 1)];
                    newData[i * channels + c]   = Mathf.Lerp(sample1, sample2, frac);
                }
            }

            return newData;
        }

        private static byte[] ConvertToWav(float[] data, int sampleRate, int channels, PCMFormatSize bitDepth)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                int byteDepth   = (int)bitDepth / 8;
                int sampleCount = data.Length;
                int byteCount   = sampleCount * byteDepth;

                // RIFF header
                stream.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"), 0, 4);
                stream.Write(BitConverter.GetBytes(36 + byteCount), 0, 4);
                stream.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"), 0, 4);

                // fmt subchunk
                stream.Write(System.Text.Encoding.ASCII.GetBytes("fmt "), 0, 4);
                stream.Write(BitConverter.GetBytes(16), 0, 4); // Subchunk1Size
                stream.Write(BitConverter.GetBytes((short)1), 0, 2); // AudioFormat PCM
                stream.Write(BitConverter.GetBytes((short)channels), 0, 2);
                stream.Write(BitConverter.GetBytes(sampleRate), 0, 4);
                stream.Write(BitConverter.GetBytes(sampleRate * channels * byteDepth), 0, 4); // ByteRate
                stream.Write(BitConverter.GetBytes((short)(channels * byteDepth)), 0, 2); // BlockAlign
                stream.Write(BitConverter.GetBytes((short)bitDepth), 0, 2);

                // data subchunk
                stream.Write(System.Text.Encoding.ASCII.GetBytes("data"), 0, 4);
                stream.Write(BitConverter.GetBytes(byteCount), 0, 4);

                // Write data
                foreach (float sample in data)
                {
                    float clamped = Mathf.Clamp(sample, -1f, 1f);
                    if (bitDepth == PCMFormatSize.SixteenBit)
                    {
                        short intSample = (short)(clamped * short.MaxValue);
                        stream.Write(BitConverter.GetBytes(intSample), 0, 2);
                    }
                    else if (bitDepth == PCMFormatSize.EightBit)
                    {
                        byte intSample = (byte)((clamped + 1f) * 127.5f);
                        stream.WriteByte(intSample);
                    }
                }

                return stream.ToArray();
            }
        }
    }
}

