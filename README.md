## SimVault — Interactieve Wachtwoordmanager Simulatie

> Bachelorproef HOGENT — Educatieve simulatie voor beginners

##  Live demo
[simvault.netlify.app](https://simvault.netlify.app)

## Vereisten
- Unity 6 (6000.3.x LTS)
- WebGL Build Support module

## Project opzetten

### Stap 1 — Repository klonen
```bash
git clone https://github.com/JOUWGEBRUIKERSNAAM/SimVault.git
```

### Stap 2 — Project openen in Unity
1. Open Unity Hub
2. Klik **Open** → selecteer de gekloonde map
3. Wacht tot Unity klaar is met importeren

### Stap 3 — Scene openen
1. Ga naar **Assets → Scenes**
2. Dubbelklik op **SampleScene**

### Stap 4 — WebGL Build
1. **File → Build Settings**
2. Selecteer **WebGL**
3. Klik **Switch Platform**
4. Klik **Build**

### Stap 5 — Lokaal testen
```bash
cd BuildMap
python -m http.server 8000
```
Open `http://localhost:8000` in Chrome.

## 📁 Projectstructuur

Assets/
├── Scripts/          # Alle C# scripts
├── Scenes/           # Unity scenes
├── Prefabs/          # UI prefabs (VaultEntryRow)
├── Images/           # Sprites en iconen
├── Plugins/
│   └── WebGL/        # JavaScript plugins (BiometricAuth.jslib)
└── WebGLTemplates/
└── SimVault/     # Aangepaste index.html

## 🔧 Aanpassingen maken

### Nieuwe fictieve website toevoegen
1. Maak een nieuw Panel aan in de Scene
2. Voeg een script toe met je paginalogica
3. Registreer de URL in `BrowserController.cs`:
```csharp
private readonly Dictionary<string, PageInfo> knownPages = new()
{
    { "sim://nieuwesite", new PageInfo("NieuweSite", "sim://nieuwesite", true) }
};
```

### Tutorial stappen aanpassen
1. Klik op **TutorialOverlay** in de Hierarchy
2. Selecteer **TutorialManager** component
3. Pas de **Steps** lijst aan in de Inspector

## Privacy
- Geen echte data wordt opgeslagen
- Alles verdwijnt bij sluiten browser
- Enige uitzondering: biometric credential ID in localStorage

## 📝 Licentie
MIT License — vrij te gebruiken en aanpassen

## 👤 Auteur
**Ward Hawinkel** — HOGENT 2025-2026

Begeleider: Gertjan Bosteels
Co-promotor: Thomas Clauwaert
