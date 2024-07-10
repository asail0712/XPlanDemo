using System;
using System.Linq;
using System.Text;

using UnityEngine;

#if PCSC 
using PCSC;
using PCSC.Iso7816;
#endif // PCSC

namespace XPlan.Utility
{
	public static class CardReader
	{
		// 健保卡號、名字、身分證字號、性別、生日、發卡日
		static public bool GetNHICardInfo(Action<string, string, string, string, string, string> finishAction)
		{
#if PCSC
			byte[] TempArray1 = new byte[12];
			byte[] TempArray2 = new byte[20];
			byte[] TempArray3 = new byte[10];
			byte[] TempArray4 = new byte[7];
			byte[] TempArray5 = new byte[1];
			byte[] TempArray6 = new byte[7];

			using (ISCardContext ctx = ContextFactory.Instance.Establish(SCardScope.User))
			{
				string firstReader = ctx.GetReaders().FirstOrDefault();

				if (firstReader == null)
				{
					return false;
				}

				using (IsoReader isoReader = new IsoReader(context: ctx, readerName: firstReader, mode: SCardShareMode.Shared, protocol: SCardProtocol.Any))
				{
					CommandApdu selectApdu = new CommandApdu(IsoCase.Case4Short, isoReader.ActiveProtocol)
					{
						CLA		= 0x00,
						INS		= 0xA4,
						P1		= 0x04,
						P2		= 0x00,
						Data	= new byte[] { 0xD1, 0x58, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x11 },
						Le		= 0x00
					};

					Console.WriteLine("Send Select APDU command: \r\n{0}", BitConverter.ToString(selectApdu.ToArray()));

					Response selectResponse = isoReader.Transmit(selectApdu);
					Console.WriteLine("SW1 SW2 = {0:X2} {1:X2}", selectResponse.SW1, selectResponse.SW2);

					CommandApdu readProfileApdu = new CommandApdu(IsoCase.Case4Short, isoReader.ActiveProtocol)
					{
						CLA		= 0x00,
						INS		= 0xCA,
						P1		= 0x11,
						P2		= 0x00,
						Data	= new byte[] { 0x00, 0x00 },
						Le		= 0x00
					};

					Response profileResponse = isoReader.Transmit(readProfileApdu);
					Console.WriteLine("SW1 SW2 = {0:X2} {1:X2}", profileResponse.SW1, profileResponse.SW2);

					if (profileResponse.HasData)
					{
						byte[] data = profileResponse.GetData();

						// 有資料，長度57 註解 by Victor

						Array.Copy(data, 0, TempArray1, 0, 12);
						Array.Copy(data, 12, TempArray2, 0, 20);
						Array.Copy(data, 32, TempArray3, 0, 10);
						Array.Copy(data, 42, TempArray4, 0, 7);
						Array.Copy(data, 49, TempArray5, 0, 1);
						Array.Copy(data, 50, TempArray6, 0, 7);

						string cardNumber		= ByteToStr(TempArray1);
						string cardHolderName	= GetHolderName(TempArray2);
						string holderIdNum		= ByteToStr(TempArray3);
						string holderBirth		= ByteToStr(TempArray4);
						string holderSex		= ByteToStr(TempArray5);
						string cardIssueDate	= ByteToStr(TempArray6);

						Debug.Log(
							$"Card Number  : { cardNumber }\r\n" +
							$"Holder Name  : { cardHolderName }\r\n" +
							$"Holder ID    : { holderIdNum }\r\n" +
							$"Holder Sex   : { holderSex }\r\n" +
							$"Holder Birth : { holderBirth }\r\n" +
							$"Card Issue On: { cardIssueDate }");

						finishAction?.Invoke(cardNumber, cardHolderName, holderIdNum, holderSex, holderBirth, cardIssueDate);

						return true;
					}
				}
			}
#endif //PCSC

			return false;
		}

		private static string ByteToStr(byte[] input)
		{
			Encoding asciiEncoding = Encoding.ASCII;
			return asciiEncoding.GetString(input);
		}

		private static string GetHolderName(byte[] input, string encodingType = "big5")
		{
			string holderName;

			EncodingInfo big5EncodingInfo = Encoding.GetEncodings().FirstOrDefault(_ => _.Name == encodingType);

            if (big5EncodingInfo == null)
            {
                // Register a Big5 coding provider
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
				Encoding encoding	= Encoding.GetEncoding(encodingType);
                holderName			= encoding.GetString(input);
            }
            else
			{
				holderName		= big5EncodingInfo.GetEncoding().GetString(input);
			}

			holderName = holderName.TrimEnd('\0');

			//NOTE: Some newer NHI cards have fill space characters to the end
			if (' ' == holderName.Last())
			{
				holderName = holderName.TrimEnd();
			}

			return holderName;
		}
	}
}