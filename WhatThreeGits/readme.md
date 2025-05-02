# WhatThreeGits

A tool to encode Git hashes into 3-word combinations, similar to the *What3Words* concept.

<div style="padding:8px 0px 8px 16px;font-size:150%;font-weight:bold;background-color:#666">TL;DR;</div>
<div style="margin:0 0 32px 0;padding:8px 8px 4px 8px;background-color:#333">

Install the tool with
```powershell
dotnet tool install -g WTG --add-source .\
```
Run with 
```powershell
> WTG encode --short		# short hash 
57ff74bcd28c -> abdu.mimzy.overdrinking

> WTG decode abdu.mimzy.overdrinking 
abdu.mimzy.overdrinking -> 57ff74bcd28c
```
To uninstall 
```powershell
dotnet tool uninstall -g WTG
```
</div>


## Dictionary size
6-word combination covers roughly **90 bits** so **~10-11** words.

## Other option - Truncate the hash

So for 3 words from a 40,000-word list **could replace short Git hashes** safely in most cases.

Git only needs **the first 7-10 characters** to uniquely identify a commit in most repos. That’s **28-40 bits**, much more manageable:
40,000^3 \approx 6.4 \times 10^{13} \text{ combinations} \Rightarrow \text{~46 bits}


## How Many 3‑Word Combinations Do We Need ?

<summary>Math&nbsp;derivations (What 3 Words vs Git hash)</summary>

### 1&nbsp; What3Words – covering the planet

Each 3 m × 3 m square must receive a unique triplet.

$$
\text{Earth surface area} \approx 510 \times 10^{6}\ \text{km}^2
$$

$$
1\ \text{km}^2 = 10^{6}\ \text{m}^2
\quad\Longrightarrow\quad
\text{squares} =
\frac{510 \times 10^{6} \times 10^{6}}{3 \times 3}
\approx 5.7 \times 10^{13}
$$

Let \$n\$ be the size of the word‑list used in **each** of the three positions:

$$
n^3 \;\ge\; 5.7 \times 10^{13}
\quad\Longrightarrow\quad
n \;\ge\; \sqrt[3]{\,5.7 \times 10^{13}\,}
\approx 3\,819
$$

&nbsp;→ A list of **≈ 4 000 words** is sufficient for global coverage.  
In practice *What3Words* uses ~40 000 words to add redundancy and error‑correction margin.

---

### 2&nbsp; Encoding a full Git SHA‑1 hash

A Git commit hash is 160 bits:

$$
\text{combinations} = 2^{160} \approx 1.46 \times 10^{48}
$$

To squeeze that into three words:

$$
n^3 = 1.46 \times 10^{48}
\quad\Longrightarrow\quad
n = \sqrt[3]{\,1.46 \times 10^{48}\,}
\approx 1.14 \times 10^{16}
$$

That would require **10 quadrillion** distinct English words—obviously impossible.

---

### 3&nbsp; Practical compromise

* **Short hashes** – Git can uniquely identify commits in most repos with 7–12 hex chars  
  (≈ 28 – 48 bits). Three words from a 40 k vocabulary:

  $$
  40\,000^3 \approx 6.4 \times 10^{13} \;>\; 2^{46}
  $$

  comfortably covers the 48‑bit space.

* **Full hashes** – With a 32 768‑word list (2¹⁵ ≈ 15 bits/word) you need  

  $$
  \lceil\,160 / 15\,\rceil = 11 \text{ words} \;(+1 \text{ CRC word})
  $$

  to represent the entire 160‑bit SHA‑1 with checksum.



