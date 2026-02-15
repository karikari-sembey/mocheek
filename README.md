# MoCheek(もちーく) - もちもちほっぺジェネレーター

![MoCheek(もちーく) Logo](./MoCheekLogo.png)

MoCheekは、VRChatのアバターたちのほっぺをもちもちにすることを目的にしたツールです。

## 使い方

### プリセットを使う方/他の人のプロファイルを使う方

Unity Editor上部のメニューバーから`Tools>MoCheek`をクリックしてください
表示されたウィンドウの説明に従って設定を進めましょう

### 自分でイチから設定したい方

MoCheekはおおよそ5ステップで設定できます！

1. アバターの顔のSkinned Mesh Rendererと同じオブジェクトに`GuraRil>MoCheek`をAdd Componentしてください
2. 追加ボーンの場所に増やしたい数ボーンを追加してください
3. Gizmoを使用してボーンの位置を調節してください
   - 現在Gizmoは根っこのボーンを設定するためにしか使えません！今後のアップデートでほっぺのルートボーンにもGizmoをつける予定です
4. もちもち範囲とウェイト減衰を調節してお好きなもちもちを表現しましょう！
5. 角度制限をつけてもちもちを破綻しづらくするのもお忘れなく :3

## 依存関係

- Unity Editor: 2022.3.22.f1
  - これ以外のUnityバージョンとの互換性は保証していません。
- VRChat SDK - Avatars: 3.10.1以降
  - 一応下位互換性はあるはずですが、**3.9.0より古いバージョンはサポートされません！**
- NDMF(Non-Destructive Modular Framework): 最新版
  - 1.9.4以降で動作確認済みですが、最新版以外はサポートしていません。
  - 以下のどちらかが導入されていれば新たに導入する必要はありません。
    - [AAO: Avatar Optimizer](https://vpm.anatawa12.com/avatar-optimizer/ja/)
    - [Modular Avatar](https://modular-avatar.nadena.dev/)
  - どちらも導入されていない場合は、Modular Avatarのリポジトリを追加することで使用できます。

## 導入方法

[ぐらりぃるのVPMリポジトリ](https://vpm.guraril.com/addrepo)からリポジトリを登録して、プロジェクトにMoCheekを追加します。
