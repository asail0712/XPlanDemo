# XPlan Framework  
基於 EDA（Event-Driven Architecture）的 Unity 中小型專案快速開發框架

---

# 1. 簡介
在建構專案時，你是否也曾為了保持功能的彈性，建立了大量的 interface，反而讓專案變得艱澀難懂？

大多數的程式碼都可以拆分為三大功能：
- 商業邏輯
- 資源管理
- 引用管理

其中，為了增加程式彈性，「引用管理」往往需要投入大量心力來設計與維護。

若有一個框架能替你處理「引用管理」，讓你能將開發重心放在「商業邏輯」與「資源管理」上，不僅能大幅節省開發時間，還能在需求變動時更加從容。

✨ 如果以上描述讓你有共鳴，那麼你一定要試試 **XPlan**。

XPlan 是一個基於 EDA 架構的 Unity 中小型快速開發框架：

- 掌握 EDA 的優勢給予代碼足夠的彈性與擴展度，同時避免過度依賴設計模式  
- 降低專案維護成本，提高開發速度  
- 使用事件導向 MVP 解構 View 與 Presenter  
- 提供聲音、網路、UI、Camera、物件管理、Debug 等工具套件  

📘 **XPlan框架介紹簡報**  
https://docs.google.com/presentation/d/19OwJzuN3nLxXHewKaFCApZNY4GO7cCcZtz5_IMY643A/edit#slide=id.g3125b255978_2_10

---

# 2. 版本資訊
- **Version 3.6.8**

---

# 3. 安裝指南

## 系統要求
- Unity **6000.0.58f2** 或更高版本

## 安裝方式
- 將 `XPlan/` 資料夾放入 `Assets/Plugins/`  
- 不使用的 DLL 可自由刪除（見「其他」說明）

---

# 4. 目標和功能

## 🎯 目標
提升 Unity 專案的開發效率，節省開發時間，強化跨場景與跨系統的溝通能力。

## ✦ 功能
- 使用簡化 EDA 架構作為系統基底  
- 支援 **MVVM** 或 **MVP** UI 架構  
  - 參考示例：https://github.com/asail0712/PlayMeowDemo
- 提供 API 與 WebSocket 套件  
- 提供多聲音播放、切換、Fade In/Out 的 Audio 套件  
- 建立場景間的關係，簡化 Scene 管理與切換  
- 提供 UI 在地化  
- 提供每個場景獨立的 Input 與手勢操作
- 提供IL Weaving處理，可自行擴充Weaver
  - 參考示例：https://github.com/asail0712/ILWeaveSurvey 
- 常用組件與工具庫  
  - 字串處理  
  - Texture 處理  
  - Singleton  
  - Recycle Pool  
  - Easing Function  
  - Web Camera 功能  
  - Gesture  
- 其他功能  
  - GPS 解析  
  - PCSC Smart Card  
  - QRCode 加解密  
  - SHA256 加密

---

# 5. 使用說明（範例場景）

### System Architecture Demo  
`Assets/Scenes/SystemArchitectureDemo/MainScene.unity`  
- 說明基礎系統架構與 UI 架構

### Input 多場景示例  
`Assets/Scenes/InputDemo/InputScene1.unity`  
- 多組 Input 設定

### SceneController + Input  
`Assets/Scenes/InputWithSceneControllerDemo/ParentScene.unity`  
- SceneController 使用方式  
- 多場景 Input 管理

### API Demo  
`Assets/Scenes/APIDemo/APIDemoScene.unity`  
- 弱連線 API 呼叫  
- 查詢氣象局溫度

### WebSocket Demo  
`Assets/Scenes/WebSocketDemoScene/WebSocketDemoScene.unity`  
- 強連線訊息傳送與接收

### Audio Demo  
`Assets/Scenes/AudioDemo/AudioDemoScene.unity`  
- 多聲道音效  
- BG 自動降低

### Microphone 音訊串流  
`Assets/Scenes/AudioDemo/MicEchoDemo.unity`  
`Assets/Scenes/AudioDemo/MicrophoneDemo.unity`

### Recycle Pool  
`Assets/Scenes/RecyclePoolDemo/RecyclePoolDemoScene.unity`

### UI Localization  
`Assets/Scenes/UILocalizationDemo/LocalizationScene.unity`

### Gesture Demo  
`Assets/Scenes/GestureDemo/GestureDemo.unity`  
- Drag / Rotate / Pinch / Tap  

### Scene Debug  
`Assets/Scenes/SceneDebugDemo/SceneDebugDemo.unity`

---

# 6. IL Weaving 簡要說明

XPlan 提供 IL Weaving 功能，能在 **編譯後、Unity 執行前** 自動修改程式組件（DLL）中的 IL 程式碼，替你注入必要功能。

其目的在於：
- **減少樣板代碼**
- **避免重複邏輯分散在多個類別**
- **降低 UI / VM / Manager 之間的綁定複雜度**
- **加快開發速度與可維護性**

藉由加入 Attribute，Weaver 可以自動：
- 將欄位轉換成 Observable Property  
- 在 Awake / OnEnable / OnDisable 注入必要流程  
- 自動綁定 UI 按鈕與 ViewModel 方法  
- 建立事件註冊與通知流程  
- 自動生成 VM 與 View 的溝通邏輯  

📘 更多 Weaving 概念與示例  
https://github.com/asail0712/ILWeaveSurvey

---

# 7. manifest 設定

為使 IL Weaving 正常執行，請在 `Packages/manifest.json` 加入：

"com.unity.nuget.mono-cecil": "1.11.6",

"com.unity.nuget.newtonsoft-json": "3.2.1",

# 8.其他
- 前置定義
  - PCSC 可開啟支援讀取Smart Card功能
  - ZXING 可開啟轉二維條碼功能
- DLL說明
  - PCSC  用於讀取Smart Card，沒有使用可移除
  - ZXING 對QRCode做加解密，沒有使用可移除

# 9.聯繫方式
Email: asail0712@gmail.com

# 10. 相關資源
### [Avatar SDK Demo : 將照片轉為3D模型](https://github.com/asail0712/AvatarSDKDemo)
### [MediaPipe Demo : 影像辨識](https://github.com/asail0712/XPlan_MediaPipeDemo)
### [AR Demo : 擴增實境](https://github.com/asail0712/XPlan_AR)
