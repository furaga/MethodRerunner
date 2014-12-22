MethodRerunner
===========================

実行時に関数の引数を保存して、実行後に復元して単体テストなどに利用できるVisual Studio拡張機能。  
ビデオを見ると雰囲気はわかると思う(https://www.youtube.com/watch?v=BGdnBHPWdS8)．  
  
内部ではFLib (https://github.com/furaga/FLib) に含まれている FLib.ForceSerializer を使って引数の保存・復元を行う．  
ソースコードのパースにはRoslyn(https://roslyn.codeplex.com/)を使っている．  
  
自分が使うのに必要な最小限の実装になっている．  
なのでジェネリック関数に対応していないなど、コーナーケースは結構ある．

- 使い方  
1) MethodRerunnerプロジェクトをビルドして、Visual Studioにインストールする．  
2) あなたが開発中のC#プロジェクトを開く．  
3) FLib (https://github.com/furaga/FLib) をプロジェクトの参照に追加する．  
4) プログラムの開始位置付近（Program.Main()の1行目など）で FLib を初期化 "FLib.FLib.Initialize();" する．  
5) 引数を保存したい関数内にブレークポイントを仕掛けてデバッグ実行．  
6) ブレークポイントに引っかかったらテキストエディタ上で右クリック -> "Save method parameters"．正常に動作すれば関数の引数値が保存される．  
6) 実行終了後、コード中の適当な位置で右クリック->"Insert method test code"．引数値の保存履歴が表示される．  
7) いずれかの項目をダブルクリック or エンターキーを押すとその引数値を復元して、関数呼び出しを行うコード片が自動挿入される。