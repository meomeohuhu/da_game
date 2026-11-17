# Asset provenance

All artwork/audio actually referenced by the runtime is checked into the repository. No paid Unity Asset Store package, external asset downloader or account is required at runtime.

| Files | Creator / source | License | Usage |
|---|---|---|---|
| `Resources/Art/kenney_*.png` | Kenney Vleugels, [Tower Defense (Top-Down)](https://kenney.nl/assets/tower-defense-top-down); downloaded from [author-attributed OpenGameArt distribution](https://opengameart.org/content/tower-defense-top-down) | CC0; original `Kenney-License.txt` alongside artwork | Tree canopy, shrubs, rocks, vehicles/drones, flame, projectile glint |
| `Resources/Art/{thorn,frost,bloom,bomb,poison,ancient}.png` | Original programmatic illustrations created for this project | CC0 dedication below | Distinct plant silhouettes and Ancient Tree |
| `Resources/Fonts/GuardianSans.ttf` | Noto Sans Regular, [Noto fonts](https://github.com/notofonts/noto-fonts/tree/main/hinted/ttf/NotoSans) | SIL OFL 1.1; bundled `OFL.txt` | Vietnamese UI typography. Filename alias only; the font is unmodified. |
| `Resources/Audio/*.wav` | Original synthesized ambient chord bed and feedback cues created for this project | CC0 dedication below | Looping ambient bed, planting, enemy destruction, wave start |

Downloaded 2026-11-17. The Kenney download contains a CC0 license with personal/commercial use permission. Selected images keep their original pixels; filenames map directly to the original `towerDefense_tileNNN.png`. The official site returned HTTP 403 in this environment, so the listed OpenGameArt distribution was used. The original archive is not required in the repository.

## Art direction

A dark jade forest, muted ochre routes, warm gold for economy/traps, cool cyan for frost and soft lilac for poison. Original plant art uses clear silhouettes and transparent backgrounds to sit beside Kenney's simple overhead shapes. UI distinguishes traps from normal pads and communicates lock/cooldown states numerically.

**Quality status:** file integrity, transparency, dimensions and references were checked; final in-engine composition, animation, legibility and sound mix still need review in Unity. Upgrade levels currently increase scale and pip markers; they do not have three separate animated sprite sets. One ambient musical bed is shared across levels. Those are art-polish tasks, not finished bespoke campaign art/audio.

## Original asset dedication

To the extent possible under law, the original plant/tree illustrations and synthesized WAV audio created for this repository are dedicated to the public domain under [CC0 1.0 Universal](https://creativecommons.org/publicdomain/zero/1.0/). This dedication does not alter third-party font or Kenney licenses. The project name and source document are not included in this dedication.

## Nocturne visual revision

Original CC0 procedural artwork now replaces the six plant/tree sprites and adds nine industrial enemy sprites (`enemy_*.png`) plus `dark-forest.jpg`. The new direction uses angular silhouettes, weathered metal, root/stone bases, restrained cyan and amber illumination, and no cartoon eyes on the towers. These assets are shared by Unity and BrowserDemo. Original dedicated assets retain the CC0 dedication above; Kenney artwork still used for rocks/flames remains under its bundled license.
