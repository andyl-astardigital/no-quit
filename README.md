```
       ##    ##    ##
       ##    ##    ##
       ##    ##    ##
      +------------------+
      |                  |
      | ##############   |--+
      | ##############   |  |
      | ##############   |--+
      |                  |
      +------------------+
       +----------------+
```

# NoQuit

A tiny tray app that keeps Windows from sleeping.

[![ci](https://github.com/andyl-astardigital/no-quit/actions/workflows/ci.yml/badge.svg)](https://github.com/andyl-astardigital/no-quit/actions/workflows/ci.yml)
[![license](https://img.shields.io/badge/license-MIT-00ff41.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4.svg)](https://dotnet.microsoft.com/)

---

## what it does

Three things, in order:

1. **`SetThreadExecutionState`** — the official Windows API. Tells the OS not to sleep.
2. **Invisible mouse nudge** (1 px right, 1 px back) every 50 s — defeats group-policy timers that ignore the API.
3. **Synthetic F15 keystroke** — no app reacts (F15 isn't on modern keyboards) but Windows counts it as input.

Auto-restarts on logon, workstation-unlock and resume-from-sleep.

---

## install

Grab `NoQuit.exe` from [Releases](https://github.com/andyl-astardigital/no-quit/releases), drop it anywhere:

```powershell
.\NoQuit.exe              # run once
.\NoQuit.exe --install    # register auto-start (logon | unlock | resume)
.\NoQuit.exe --uninstall  # remove
```

Look for the green coffee cup in your tray. Left-click toggles, double-click opens the console, right-click for the menu.

Console hotkeys: `SPACE` toggle, `ESC` close, `CTRL+Q` kill daemon.

---

## architecture

Three projects:

```
no-quit/
  src/
    NoQuit.Core/        net8.0, no IO. interfaces, reducer, models, helpers.
    NoQuit/             net8.0-windows. adapters, ui, composition root.
  tests/
    NoQuit.Tests/       xunit + FluentAssertions. hand-rolled fakes.
```

State changes happen in one place: a pure reducer.

```
DaemonReducer.Reduce : (State, Event, Now) -> (State', Effect[])
```

`Daemon` runs the reducer and hands the effects to `EffectInterpreter`, which maps each effect to an interface call. Every side effect is interface-typed — `IPowerApi`, `IInputSynthesizer`, `ITrayShell`, `IConsoleHost`, `ITaskScheduler`, etc — so the orchestration is testable end-to-end without touching real IO. The concrete implementations live in `NoQuit/Adapters/` and `NoQuit/Ui/`.

---

## build

```powershell
git clone https://github.com/andyl-astardigital/no-quit
cd no-quit

dotnet test
dotnet publish src/NoQuit/NoQuit.csproj -c Release -o dist
```

Requires .NET 8 SDK. Output is `dist/NoQuit.exe` — single-file, self-contained, ~68 MB. No .NET runtime needed on the target machine.

---

## license

MIT — see [LICENSE](LICENSE).
