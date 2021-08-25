# StockTelegramBot

台北股市即時股價查詢機器人

## 簡介

---

### 如何使用?

在Telegram搜尋 ID:@TSRTQ_bot

![sarch](https://raw.githubusercontent.com/zccheng8320/StockTelegramBot/master/img/sarch.png)

### 目前支援指令

1. 個股報價 : /t (個股名稱或代號)

    ``` telegram command
    查詢個股 :
    /t 2330 或 /t 台積電
    查詢加權指數 :
    /t 大盤 或 /t 加權指數
    查詢櫃買指數:
    /t 櫃買 或 /t 櫃買指數
    ```

    ![text](https://raw.githubusercontent.com/zccheng8320/StockTelegramBot/master/img/text.png)

2. 股價走勢圖 : /c  (個股名稱或代號)

    ``` telegram command
    查詢個股 :
    /c 2330 或 /c 台積電
    查詢加權指數 :
    /c 大盤 或 /c 加權指數
    查詢櫃買指數:
    /c 櫃買 或 /c 櫃買指數
    ```

    ![chart](https://raw.githubusercontent.com/zccheng8320/StockTelegramBot/master/img/chart.png)

## 開始前

---

1. 必須安裝 .NET 5.0 SDK，連結[在此](https://dotnet.microsoft.com/download)，請根據執行環境的作業系統下載。
2. 因為有使用到Chrome Web Driver，因此必須執行環境必須要有安裝[Chrome](https://www.google.com/intl/zh-TW/chrome/?brand=FKPE&gclid=Cj0KCQjwsZKJBhC0ARIsAJ96n3VDJ_X1xFONlf40caWSU3EacqzJ_XdvoWRnYGD1z0ZCja0hc3AY0AQaAvOIEALw_wcB&gclsrc=aw.ds)(版本 92.0 以上)。
3. 建立自己的Telegram Bot ，透過Telegram 提供的Bot Father 取得 Telegram Bot token，詳細步驟請參考此[連結](https://core.telegram.org/bots#6-botfather)


## Quick Start

---

> 此範例使用LongPolling模式運行(主動式，不需建立Server監聽Request)，但Webhook模式設定方式大同小異，只差在appsetting.json需要設定您的Webhook的Url，詳細可以參考此[連結](https://github.com/zccheng8320/TelegramBotExtensions)。

1. 透過Git，下載專案，也可以手動下載。

    ```bash
    git clone https://github.com/zccheng8320/StockTelegramBot.git
    ```

2. 編譯StockTelegramBot Solution。

    ```bash
    cd StockTelegramBot

    dotnet build
    ```

3. 到LongPolling 資料夾。

    ```bash
    cd StockTelegramBot/LongPolling
    ```

4. 開啟appsetting.json，並在TelegramApiToken 輸入你的telegram bot api token。

    ```json
    {
        "Logging": {
            "LogLevel": {
                "Default": "Debug",
                "System": "Information",
                "Microsoft": "Information"
            }
        },
        "TelegramSetting": {
            "TelegramApiToken": "your api token"
        }
    }
    ```

5. 執行LongPolling Project，在LongPolling 的資料夾輸入以下指令。

    ```bash
    dotnet run
    ```

6. 運行起來後，就可以在Telegram上查詢囉。

![demo](https://raw.githubusercontent.com/zccheng8320/StockTelegramBot/master/img/demo.png)
