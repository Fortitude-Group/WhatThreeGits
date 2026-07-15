<div align="center">

# WhatThreeGits

**Encode a Git commit hash into three memorable words — and back again, exactly.**

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20macOS%20%7C%20Linux-0d9488?style=flat-square)](#install)
[![License: MIT](https://img.shields.io/badge/License-MIT-0d9488?style=flat-square)](LICENSE)
[![Website](https://img.shields.io/badge/website-whatthreegits-2dd4bf?style=flat-square&logo=githubpages&logoColor=white)](https://whatthreegits.fortitude-omnis.group)

</div>

`57ff74bcd28c` is a nightmare to read aloud. `abdu.mimzy.overdrinking` isn't.

**WhatThreeGits** (`WTG`) is a small .NET global tool that turns the short hash Git actually
needs into three speakable words — and decodes those words straight back to the hash,
deterministically. Say your commit in a standup, drop it in a ticket, and never spell out
`5-7-f-f-7-4` character by character again. It's the [What3Words](https://what3words.com)
idea, applied to commits.

🌐 **Product site:** <https://whatthreegits.fortitude-omnis.group>

---

## Install

```powershell
dotnet tool install -g WTG --add-source .\
```

## Usage

```powershell
> WTG encode --short                       # short hash
57ff74bcd28c -> abdu.mimzy.overdrinking

> WTG decode abdu.mimzy.overdrinking
abdu.mimzy.overdrinking -> 57ff74bcd28c
```

## Uninstall

```powershell
dotnet tool uninstall -g WTG
```

---

## How it works

WhatThreeGits maps the part of a hash Git actually needs (the first 7–12 hex characters —
about 28–48 bits) onto three positions drawn from a ~40,000-word list. The mapping is
deterministic and fully reversible: the same hash always produces the same words, and the
same words always decode back to the exact hash. No lookup service, no database, no network.

## Why three words is enough (the math)

The same reasoning behind What3Words, which gives every 3 m × 3 m square on Earth a unique
triplet of words.

**1 — Covering the planet.** Earth's surface is about $510 \times 10^{6}\ \text{km}^2$, so the
number of 3 m × 3 m squares is

$$
\frac{510 \times 10^{6} \times 10^{6}}{3 \times 3} \approx 5.7 \times 10^{13}.
$$

To give each a unique triplet from a word list of size $n$ you need
$n^3 \ge 5.7 \times 10^{13}$, i.e. $n \ge \sqrt[3]{5.7 \times 10^{13}} \approx 3{,}819$ —
about **4,000 words** covers the globe. What3Words uses ~40,000 for redundancy.

**2 — A full Git SHA-1** is 160 bits, or $2^{160} \approx 1.46 \times 10^{48}$ combinations.
Squeezing that into three words would need $\sqrt[3]{1.46 \times 10^{48}} \approx 1.14 \times 10^{16}$
words per position — 10 quadrillion. Three words can't hold a full hash.

**3 — The practical compromise.**

- **Short hashes.** Git resolves a commit from 7–12 hex characters in most repos (≈ 28–48 bits).
  Three words from a 40k vocabulary swallow that whole:

$$
40{,}000^3 \approx 6.4 \times 10^{13} > 2^{46},
$$

  comfortably covering the 48-bit space. This is the everyday `--short` mode.

- **Full hashes.** With a 32,768-word list ($2^{15} \approx 15$ bits per word) the entire
  160-bit SHA-1 fits in $\lceil 160 / 15 \rceil = 11$ words, plus one CRC word for error
  detection.

---

## License

Released under the [MIT License](LICENSE). Copyright © 2026 Fortitude Omnis Group Ltd.

<div align="center">

A **[Fortitude Omnis Group](https://fortitude-omnis.group)** product.

</div>
