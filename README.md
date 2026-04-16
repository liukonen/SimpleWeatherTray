
# Weather Widget System

**A Modular, Zero-Bloat Desktop Client**

I built this because I wanted a weather widget that actually stayed out of the way. Most weather apps are bloated, resource-heavy, and break the second an API changes. This is my solution to that.

### Tech Stack

-   **Runtime:** C# / .NET
    
-   **UI:** GTK (Native Desktop)
    
-   **Architecture:** Plugin-based (Interfaces & DTOs)
    

### The Mission

The goal was simple: **Stability over features.** I’ve seen too many projects suffer from "feature creep" until they become unstable. I architected this so the core widget knows absolutely nothing about where the data comes from. It only knows the interfaces. If the National Weather Service changes their API tomorrow, I just swap one plugin file. The rest of the app never even notices.

### Key Decisions

-   **Interface-First:** Everything is decoupled. The UI is agnostic, meaning it doesn't care if the data comes from a gov API or a local sensor.
    
-   **The Trade-off:** It took more work upfront to build the plugin loading system, but it saved me from ever having to refactor the main app when a third-party vendor changes their JSON.
    
-   **Low Footprint:** It’s a background task. It should use near-zero resources, so I kept the feature list small and the execution tight.
    

### Why it Matters

-   **Easy Maintenance:** API changes don't break the whole system—just the adapter.
    
-   **Performance:** It’s lightweight enough to run in the background 24/7 without you noticing.
    
-   **Reliability:** It’s built to survive external changes, not just look good for a week.
    

### Quick Start

Bash

```
dotnet build SimpleWeatherTrayGtkEdition/SimpleWeatherTrayGtkEdition.csproj
dotnet run --project SimpleWeatherTrayGtkEdition/SimpleWeatherTrayGtkEdition.csproj
```