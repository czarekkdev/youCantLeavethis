# youCantLeavethis 🇵🇱

> A Windows application you **cannot** close. Made in Poland with ❤️ and malice.

![Made in Poland](https://img.shields.io/badge/Made%20in-Poland-DC143C?style=for-the-badge)
![Language](https://img.shields.io/badge/Language-C%23-239120?style=for-the-badge&logo=csharp)
![Platform](https://img.shields.io/badge/Platform-Windows-0078D6?style=for-the-badge&logo=windows)
![Soundtrack](https://img.shields.io/badge/Soundtrack-Mazurek%20Dąbrowskiego-white?style=for-the-badge)

---

## What is this?

A prank application for Windows that is **genuinely very hard to close**. Once launched, it will:

- 🎵 Play the Polish national anthem on loop
- 🛡️ Mark itself as a **critical system process** (force-killing it = instant BSOD)
- 📅 Register itself in **Task Scheduler** to survive reboots
- 🔪 Kill **Task Manager**, **Process Hacker**, `taskkill`, and `tasklist` the moment they open
- 🔁 Restore the autostart entry every 200ms if deleted
- 🔁 Re-apply the critical process flag every 200ms if removed

There is exactly **one** way to exit the application.

---

## The only way out

Click the button.

> **"I give up.. And i am gay"**

Then answer honestly. You know what to do.

---

## How it works

| Mechanism | Description |
|-----------|-------------|
| `RtlSetProcessIsCritical` | Marks the process as critical — Windows BSODs on kill |
| `IsProcessCritical` | Monitors its own critical status every 200ms |
| `schtasks /create` | Registers `CET` task to run on every logon at highest privilege |
| `loop_kill` thread | Kills Task Manager & Process Hacker every 20ms |
| `check_reg` thread | Restores the scheduler task every 200ms if removed |
| `FormClosing` override | Blocks every attempt to close the window |
| `SoundPlayer.PlayLooping` | Mazurek Dąbrowskiego. Forever. |

### Safe exit sequence in `fix()`

Order is **critical**. Wrong order = BSOD.

```
1. Stop watchdog threads
2. Remove Task Scheduler entry  
3. Remove critical process flag  ← must happen BEFORE Kill()
4. Play the reward sound effect
5. Process.Kill()
```

---

## Requirements

- Windows (tested on Windows 10/11)
- Administrator privileges (required for `RtlSetProcessIsCritical` and `schtasks /rl highest`)
- .NET Framework / WinForms
- A victim

---

## Building

Open in Visual Studio, build in Release mode, run as Administrator.

> ⚠️ **Do not run this on your own machine unless you know what you're doing.**  
> Force-killing the process **will** trigger a BSOD (`CRITICAL_PROCESS_DIED`).  
> Always test in a VM.

---

## Disclaimer

This is a **joke/prank project** created for educational and entertainment purposes. The author takes no responsibility for any BSODs, emotional damage, or strained friendships resulting from use of this software.

---

*czarekczaro — Made in Poland 🇵🇱*
