# 1.簡介
用於Unity的程式框架，著重在降低代碼相依性與提高模組的覆用。
 
# 2.安裝指南
### 系統要求
- Unity 2021.3.6f1 或更高版本。
### 安裝指南
將XPlan資料夾放進專案中的Assets/Plugins即可。
  
# 3.使用說明
- Assets/Scenes/InputDemo/InputScene1.unity
  - 說明InputManager如何支援多個場景對應多個Input設定 
- Assets/Scenes/InputWithSceneControllerDemo/ParentScene.unity
  - 示範XPlan的SceneController如何設定載入與卸載關卡
  - 示範InputManager如何使用在有SceneController環境
 
# 4.目標和功能
### 目標
透過使用XPlan，降低代碼的相依性，並簡化使用Unity的功能，以加速專案的開發。
### 功能
- 類別間使用觀察者模式與中介者來降低代碼間的相依性
- 減少使用複雜的設計模式來增加代碼的閱讀性
- 將UI與前台邏輯解構，避免因為UI的頻繁更動造成代碼大幅的改動
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
  - GPS資料解析
  - PCSC支援
  - 可透過前置處理器開啟美顏API的功能
# 5.其他
無

# 6.聯繫方式
Email: asail0712@gmail.com

# 7. 附加資源
無
