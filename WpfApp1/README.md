# 通信ライブラリとテストアプリケーション

TCP、UDP、シリアル通信を統一的なインターフェースで扱うライブラリとそのテストアプリケーションです。

## 機能

- TCP/IP通信
- UDP通信
- シリアル通信
- 複数NICのサポート
- イベントベースの非同期通信
- 包括的なエラーハンドリング
- ログ機能
- 通信統計と診断機能

## プロジェクト構成

```
WpfApp1/
└── Connections/
    ├── Interfaces/
    │   ├── IConnection.cs      # 基本通信インターフェース
    │   ├── ILogger.cs         # ログ機能インターフェース
    │   └── IConnectionStatistics.cs # 統計情報インターフェース
    ├── Models/
    │   ├── ConnectionEventArgs.cs # イベント用データ構造
    │   ├── NetworkSettings.cs    # TCP/UDP設定
    │   └── SerialSettings.cs    # シリアル通信設定
    ├── Implementations/
    │   ├── TcpConnection.cs    # TCP実装
    │   ├── UdpConnection.cs    # UDP実装
    │   └── SerialConnection.cs # シリアル通信実装
    ├── Logging/
    │   └── FileLogger.cs      # ファイルログ実装
    └── Statistics/
        └── ConnectionStatistics.cs # 統計情報実装
```

## 使用方法

### TCP通信の例

```csharp
// 設定
var settings = new NetworkSettings("192.168.1.100", 8080);
var logger = new FileLogger("connection.log");
var connection = new TcpConnection(settings, logger);

// イベントハンドラの設定
connection.OnDataReceived += (sender, e) => {
    Console.WriteLine($"データ受信: {BitConverter.ToString(e.Data)}");
};

connection.OnError += (sender, e) => {
    Console.WriteLine($"エラー: {e.Message}");
};

// 接続と送信
await connection.Connect();
await connection.SendAsync(new byte[] { 0x01, 0x02, 0x03, 0x04 });
await connection.DisconnectAsync();
```

### UDP通信の例

```csharp
var settings = new NetworkSettings("192.168.1.100", 8080);
var connection = new UdpConnection(settings, new FileLogger("udp.log"));
// 以下、TCPと同様
```

### シリアル通信の例

```csharp
var settings = new SerialSettings("COM1", 9600)
{
    DataBits = 8,
    Parity = Parity.None,
    StopBits = StopBits.One
};
var connection = new SerialConnection(settings, new FileLogger("serial.log"));
// 以下、TCPと同様
```

## テストアプリケーション

- 通信方式の選択
- 接続パラメータの設定
  - TCP/UDP: IPアドレス、ポート、使用NIC
  - シリアル: ポート、ボーレート
- テストデータの送信
- 受信データの表示
- ログ表示

## 実装の特徴

1. 非同期処理
   - すべての通信処理が非同期
   - UI処理のブロッキングを防止
   - Task.Runを適切に使用

2. エラーハンドリング
   - 接続エラー
   - 送受信エラー
   - タイムアウト処理
   - イベントベースの通知

3. スレッド安全性
   - 適切な同期処理
   - イベント発火の適切な処理
   - リソースの適切な解放

4. ログ機能
   - 送受信データの記録
   - エラー情報の記録
   - タイムスタンプ付きログ

5. 統計情報
   - 送受信バイト数
   - エラー数
   - 接続時間
   - 最終エラー時刻

## 必要要件

- .NET 8.0
- System.IO.Ports パッケージ（シリアル通信用）
