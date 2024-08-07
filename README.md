# 1.簡介
基於資料導向(Data-Oriented)為基礎，並兼具物件導向(Object-Oriented)優勢，用於Unity開發的程式框架，著重在降低代碼耦合度與提高模組的覆用性，並將開發的重點放在邏輯的撰寫，而不是引用的傳遞與管理。
 
# 2.安裝指南
### 系統要求
- Unity 2021.3.6f1 或更高版本。
### 安裝指南
- 將XPlan資料夾放進專案中的Assets/Plugins即可
- 不需要使用的DLL可以刪除，請參照"其他"的說明
  
# 3.使用說明
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
 
# 4.目標和功能
### 目標
降低代碼的相依性，並加速專案的開發以及減少維護的時間。
### 功能
- 減少使用複雜的設計模式來增加代碼的閱讀性，將開發的重點放在邏輯的撰寫，而不是引用的傳遞與管理
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
- 其他 
  - GPS資料解析
  - PCSC支援
  - 二維條碼加解密
  - SH256加密
# 5.其他
- 前置處理器
  - PCSC 可開啟支援讀取Smart Card功能
  - ZXing 可開啟轉二維條碼功能
  - USE_OPENCV 開啟OpenCV的美顏功能
- DLL說明
  - PCSC、System 該DLL用於讀取Smart Card，沒有使用可移除
  - ZXing 該DLL用於將字串轉為二維條碼，沒有使用可移除

# 6.聯繫方式
Email: asail0712@gmail.com

# 7. 附加資源
無
