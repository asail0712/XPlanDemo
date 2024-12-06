# 1.簡介
一個基於EDA架構的Unity中小型專案快速開發框架，協助開發者不需精通設計模式也給予代碼足夠的靈活性與擴展度。
### [投影片介紹 : 親愛的， 我把interface丟掉了](https://docs.google.com/presentation/d/19OwJzuN3nLxXHewKaFCApZNY4GO7cCcZtz5_IMY643A/edit#slide=id.g3125b255978_2_10)
### [Avatar SDK Demo : 將照片轉為3D模型]([https://docs.google.com/presentation/d/19OwJzuN3nLxXHewKaFCApZNY4GO7cCcZtz5_IMY643A/edit#slide=id.g3125b255978_2_10](https://github.com/asail0712/AvatarSDKDemo))
# 2.版本資訊
- Version 2.0.2
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
降低專案開發難度，並節省專案的開發時間。
### 功能
- 使用簡化的EDA架構微系統建構的基礎
- 使用MVVM設計模式解構UI與Client的邏輯
- XPlan功能都放置在主執行緒上，並允許開發時使用多執行緒
- 提供強連線與弱連線的套件組
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
  - PCSC  用於讀取Smart Card，沒有使用可移除
  - ZXING 對QRCode做加解密，沒有使用可移除

# 7.聯繫方式
Email: asail0712@gmail.com

# 8. 附加資源
無
