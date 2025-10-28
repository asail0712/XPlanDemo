# 1.簡介
在建構專案時，你是否也曾為了保持功能的彈性，建立了大量的 interface，反而讓專案變得艱澀難懂？

大多少的程式碼都可以拆分為三大功能：
- 商業邏輯
- 資源管理
- 引用管理

其中，為了增加程式的彈性，「引用管理」往往需要投入大量心力來設計與維護。

若有一個開發框架能替你處理「引用管理」，讓你能將開發重心放在「商業邏輯」與「資源管理」，不僅能大幅節省開發時間，還能在需求變動時更加從容應對。

✨ 如果以上描述有引起你的興趣，那麼你一定要來看看 XPlan。

XPlan是一個基於EDA架構的Unity中小型專案快速開發框架，要點如下:
- 掌握EDA的優勢來給予代碼足夠的靈活性與擴展度，同時鬆綁使用者對設計模式的過多依賴。
- 降低專案的維護成本與提高開發速度。
- 配套的UI系統，簡單套用MVP解構UI與前端邏輯。
- 提供聲音、網路、UI、Camera、物件管理與Debug功能等功能的套件。
  
### [XPlan框架介紹 : 親愛的， 我把interface丟掉了](https://docs.google.com/presentation/d/19OwJzuN3nLxXHewKaFCApZNY4GO7cCcZtz5_IMY643A/edit#slide=id.g3125b255978_2_10)
# 2.版本資訊
- Version 2.4.8
# 3.安裝指南
### 系統要求
- Unity 6000.0.58f2 或更高版本
### 安裝指南
- 將XPlan資料夾放進專案中的Assets/Plugins即可
- 不需要使用的DLL可以刪除，請參照"其他"的說明
  
# 4.使用說明
- Assets/Scenes/SystemArchitectureDemo/MainScene.unity
  - 此為簡易計算機的範例
  - 說明XPlan的系統架構該如何建立以及類別間的溝通方式
  - 說明UI如何建立以及使用
- Assets/Scenes/InputDemo/InputScene1.unity
  - 示範InputManager如何支援多個場景對應多個Input設定 
- Assets/Scenes/InputWithSceneControllerDemo/ParentScene.unity
  - 示範XPlan的SceneController如何設定載入與卸載關卡
  - 示範InputManager如何在有SceneController環境的地方使用
- Assets/Scenes/APIDemo/APIDemoScene.unity
  - 示範XPlan的弱連線使用方式
  - 透過API連線到氣象局查詢特定地區的溫度
- Assets/Scenes/WebSocketDemoScene/WebSocketDemoScene.unity
  - 示範XPlan的強連線功能
  - 透過Websocket連線到公共測試Server，傳送並接收訊息
- Assets/Scenes/AudioDemo/MicEchoDemo.unity
  - 使用麥克風做音訊串流接收
  - 使用Websocket做音訊串流傳輸與接收
- Assets/Scenes/AudioDemo/AudioDemoScene.unity
  - 示範Audio的使用方式
  - 一個Channel同時只撥放一個聲音，後播放會強制結束前面撥放的聲音
  - 標示為BG的Channel在有其他聲音撥放時，會自動變小聲
- Assets/Scenes/AudioDemo/MicrophoneDemo.unity
  - 使用麥克風做音訊串流接收
- Assets/Scenes/RecyclePoolDemo/RecyclePoolDemoScene.unity
  - 示範如何對GameObject使用RecyclePool
- Assets/Scenes/UILocalizationDemo/LocalizationScene.unity
  - 示範UI在地化處理方式
  - 示範如何使用UIController、UILoader與UIStringTable
- Assets/Scenes/GestureDemo/GestureDemo.unity
  - 示範手勢功能如何使用
  - 包含Drag to Move，Drag to Rotate、Pinch to Zoom、Tap to Point
- Assets/Scenes/SceneDebugDemo/SceneDebugDemo.unity
  - 示範專案中的每個Scene要如何設定，讓單一場景可以獨立運作
 
# 5.目標和功能
### 目標
降低專案開發難度，並節省專案的開發時間。
### 功能
- 使用簡化的EDA架構微系統建構的基礎
- 使用MVP解構UI與Client的邏輯
- 提供API與WebSocket的套件組
- 提供聲音套件，支援多個聲音的播放與切換以及Fade In/Out
- 建立場景間的關係，簡化場景管理與切換
- 提供UI的在地化處理
- 提供每個場景的獨立按鍵輸入以及手勢輸入
- 提供常用元件與函式庫，包含
  - 字串處理
  - Texture處理
  - Singleton
  - Recycle Pool
  - Easing Functon
  - Web Camera設定
  - Gesture 功能
- 其他 
  - GPS資料解析
  - PCSC支援
  - 二維條碼加解密
  - SH256加密
# 6.其他
- 前置定義
  - PCSC 可開啟支援讀取Smart Card功能
  - ZXING 可開啟轉二維條碼功能
- DLL說明
  - PCSC  用於讀取Smart Card，沒有使用可移除
  - ZXING 對QRCode做加解密，沒有使用可移除

# 7.聯繫方式
Email: asail0712@gmail.com

# 8. 相關資源
### [Avatar SDK Demo : 將照片轉為3D模型](https://github.com/asail0712/AvatarSDKDemo)
### [MediaPipe Demo : 影像辨識](https://github.com/asail0712/XPlan_MediaPipeDemo)
### [AR Demo : 擴增實境](https://github.com/asail0712/XPlan_AR)
