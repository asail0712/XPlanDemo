# 1.簡介
用於Unity的程式框架，著重在降低代碼相依性與提高模組的覆用，並將開發的重點放在邏輯的撰寫，而不是引用的傳遞與管理。
 
# 2.安裝指南
### 系統要求
- Unity 2021.3.6f1 或更高版本。
### 安裝指南
- 將XPlan資料夾放進專案中的Assets/Plugins即可
- 不需要使用的DLL可以刪除，請參照"其他"的說明
  
# 3.使用說明
- Assets/Scenes/SystemArchitectureDemo/MainScene.unity
  - 透過建置簡易計算機的範例，說明XPlan的系統架構該如何建立。
  - 透過建置簡易計算機的範例，說明XPlan的UI該如何建立。
- Assets/Scenes/InputDemo/InputScene1.unity
  - 示範InputManager如何支援多個場景對應多個Input設定 
- Assets/Scenes/InputWithSceneControllerDemo/ParentScene.unity
  - 示範XPlan的SceneController如何設定載入與卸載關卡
  - 示範InputManager如何使用在有SceneController環境
- Assets/Scenes/APIDemo/APIDemoScene.unity
  - 示範XPlan的弱連線使用方式
  - 透過API連線到氣象局查詢特定地區的溫度
- Assets/Scenes/WebSocketDemoScene/WebSocketDemoScene.unity
  - 示範XPlan的強連線功能
  - 透過Websocket連線到公共測試Server，傳送並接收訊息
- Assets/Scenes/UILocalizationDemo/LocalizationScene.unity
  - 示範UI在地化處理方式
  - 示範如何使用UIController、UILoader與UIStringTable
 
# 4.目標和功能
### 目標
透過使用XPlan，降低代碼的相依性，並簡化使用Unity的功能，以加速專案的開發。
### 功能
- 類別間使用觀察者模式與中介者來降低代碼間的相依性
- 減少使用複雜的設計模式來增加代碼的閱讀性，將開發的重點放在邏輯的撰寫，而不是引用的傳遞與管理
- 將UI與前台邏輯解構，避免因為UI的頻繁更動造成代碼大幅的改動
- 使用StartCoroutine替代async/await的使用
- Unity很多功能無法在子執行緒上使用，因此XPlan功能都放置在主執行緒上，但是允許開發時使用多執行緒
- 提供強連線與弱連線的套件組，降低網路功能的使用難度
- 提供聲音套件，支援多個聲音的播放與切換以及Fade In/Out
- 建立場景間的關係，提供場景管理與切換
- 提供UI的在地化處理
- 提供不同場景有設定不同的按鍵輸入
- 提供常用元件
- 提供常用函式庫，包含
  - 字串處理
  - Texture處理
  - Singleton
  - RecyclePool
  - Easing Functon
- 其他 
  - GPS資料解析
  - PCSC支援
  - QrCode生成
# 5.其他
- 前置處理器
  - PCSC 可開啟支援讀取Smart Card功能
  - ZXing 可開啟轉二維條碼功能
  - USE_OPENCV 開啟OpenCV的美顏功能
- DLL說明
  - PCSC、System 沒有需要讀卡可移除
  - ZXing 沒有需要轉二維調整可移除

# 6.聯繫方式
Email: asail0712@gmail.com

# 7. 附加資源
無
