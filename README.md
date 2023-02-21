# opencv_burr_size_analyzer
Application for analyzing burr sizes in production, using OpenCV

Original task description:

**Sorjaméret meghatározás mikroszkóp felvételek alapján**

A feladat Póka Györgytől származik (BME Gyártástudományi és -technológiai Tanszék), aki a megoldást tényleges ipari célokkal tudná hasznosítani. <br>
Fém lemezek megmunkálásakor, levágásakor a perem sorjás lesz és mikroszkóp felvétel alapján meg kell mondani, hogy mekkora ez a sorja. Konkrétan a vágási felületre merőlegesen kell megmérni, hogy mekkora fém részek állnak ki. Mivel ez egy csomó méret érték, a végeredmény ezek hisztogramja (melyik értékből mennyi van), amiből aztán lehet átlagot, szélsőértékeket, percentiliseket számolni. (Pl. a 90% percentilis az a méret, ami alatt van az esetek 90%-a.)

A kiindulási adatok: a mikroszkóp felvételek (RGB képek), a felvételek pontos pozíciója a munkadarabon (számos kép készül és később fontos lesz, hogy melyik helyileg hol készült), valamint egyenlettel meg van adva, hogy hol van a CNC gép számára megadott vágási felület, amihez képest a sorja méretet kell számolni.

![res_fz025ap1s2_06](https://user-images.githubusercontent.com/100433458/220384203-88841122-bb51-4a31-9bfd-687c17453537.jpg)
![image](https://user-images.githubusercontent.com/100433458/220384336-0fa2ebf7-0a17-421d-82aa-3db16d60f7e5.png)
