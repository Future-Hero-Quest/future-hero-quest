# Future Hero Quest 四线程 handoff prompts

更新时间：2026-05-02 10:40 UTC+8

当前公共 Git 快照：

```text
repo: E:\黑客松\FutureHeroQuest
branch: dev / main
HEAD: 37eca84 chore(release): promote dev snapshot to main
remote: origin/dev = origin/main = 37eca84
```

使用方式：分别把下面 4 个文件内容复制到 4 个新线程。

| 线程 | Prompt 文件 | 建议 Git 分支 |
|---|---|---|
| 第 1 关 | `level1-bridge-thread.md` | `feature/l1-bridge-feedback` |
| 第 2 关 | `level2-archive-thread.md` | `feature/l2-archive-feedback` |
| 第 3 关 | `level3-clubroom-thread.md` | `feature/l3-clubroom-feedback` |
| 美术音频 | `art-audio-thread.md` | `feature/art-audio-pass` |

总原则：

- 不直接 push `main` / `dev`，只 push 自己的 `feature/*` 分支。
- 不改其他线程负责的 `.unity` 场景。
- 不提交 `Library/`、`Temp/`、`Logs/`、构建产物、Photon AppID。
- 玩法同步只传语义状态：`StateKey` / `StateValue`，不要同步每帧物理。
- 每次提交前跑 Unity 编译检查，至少保证 Console 无红错。

