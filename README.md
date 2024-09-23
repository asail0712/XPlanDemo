# 1.簡介
為加快專案開發速度並減少維護成本，基於資料導向(Data-Oriented)為基礎，並兼具物件導向(Object-Oriented)優勢，設計出用於Unity的程式開發框架。著重在降低代碼耦合度與提高模組的覆用性，並將開發的重點放在邏輯的撰寫，而不是引用的傳遞與管理。
# 2.版本資訊
- Version 1.8.10
# 3.安裝指南
### 系統要求
- Unity 2022.3.33f1 或更高版本
- Unity 2021.3.6f1
  - AR相關功能可能無法正常運作
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
- Assets/Scenes/AudioDemo/AudioDemoScene.unity
  - 示範Audio的使用方式
  - 一個Channel同時只撥放一個聲音，後播放會強制結束前面撥放的聲音
  - 標示為BG的Channel在有其他聲音撥放時，會自動變小聲
- Assets/Scenes/RecyclePoolDemo/RecyclePoolDemoScene.unity
  - 示範如何對GameObject使用RecyclePool
- Assets/Scenes/UILocalizationDemo/LocalizationScene.unity
  - 示範UI在地化處理方式
  - 示範如何使用UIController、UILoader與UIStringTable
- Assets/Scenes/GestureDemo/GestureDemo.unity
  - 示範手勢功能如何使用
  - 包含Drag to Move，Drag to Rotate、Pinch to Zoom、Tap to Point
- Assets/Scenes/ARDemo/ARDemoScene
  - 示範如何透過XPlan使用AR Foundation
- Assets/Scenes/SceneDebugDemo/SceneDebugDemo.unity
  - 示範專案中的每個Scene要如何設定，讓單一場景可以獨立運作
 
# 5.目標和功能
### 目標
降低代碼的相依性，並加速專案的開發以及減少維護的時間。
### 功能
- 邏輯只與資料相依，而不與邏輯相依，因此增加邏輯的複用性，也避開使用複雜設計模式的機會，代碼的可讀性因此提升
- 將開發的重點放在邏輯的撰寫，而不是引用的傳遞與管理
- 使用MVVM架構解構UI與前台邏輯，避免因為UI的頻繁更動造成代碼大幅的改動
- 使用StartCoroutine替代async/await的使用
- Unity很多功能無法在子執行緒上使用，因此XPlan功能都放置在主執行緒上，並允許開發時使用多執行緒
- 提供強連線與弱連線的套件組，降低網路功能的使用難度
- 提供聲音套件，支援多個聲音的播放與切換以及Fade In/Out
- 建立場景間的關係，提供場景管理與切換
- 提供UI的在地化處理
- 每個場景可設定不同的按鍵輸入
- 提供常用元件
- 提供常用函式庫，包含
  - 字串處理
  - Texture處理
  - Singleton
  - Recycle Pool
  - Easing Functon
  - Web Camera設定
  - Gesture 功能
  - AR相關組件
- 其他 
  - GPS資料解析
  - PCSC支援
  - 二維條碼加解密
  - SH256加密
# 6.其他
- 前置定義
  - PCSC 可開啟支援讀取Smart Card功能
  - ZXING 可開啟轉二維條碼功能
  - USE_OPENCV 開啟OpenCV的美顏功能
  - AR_FOUNDATION 可開啟AR功能
- DLL說明
  - PCSC、System 該DLL用於讀取Smart Card，沒有使用可移除
  - ZXING 該DLL用於將字串轉為二維條碼，沒有使用可移除

# 7.聯繫方式
Email: asail0712@gmail.com

# 8. 附加資源
無
