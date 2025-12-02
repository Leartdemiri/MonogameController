# MonogameController

MonogameController est une application **MonoGame** permettant de visualiser en temps réel les actions d’une **8BitDo Ultimate 2C Wired Controller**, sans utiliser d’images de manette.  
Tous les boutons, sticks analogiques et triggers sont représentés graphiquement avec des cercles, des points et des barres.

---

##  Fonctionnalités

- Affichage en temps réel de l’état des boutons principaux : **A, B, X, Y, Start, Back, L1, R1, L3, R3, L4, R4**
- Visualisation du **D-Pad** (haut, bas, gauche, droite)
- Représentation des **sticks analogiques** avec un cercle et un point indiquant la position X/Y
- Visualisation des **triggers** comme des barres proportionnelles à la pression (L2 / R2)
- Les boutons deviennent **rouges lorsqu’ils sont pressés**
- Compatible avec toutes les manettes supportant **XInput** ou **DirectInput**

---

##  Visualisation

- **Boutons** : cercles avec le texte du bouton à l’intérieur  
- **Sticks** : cercle avec un petit point mobile indiquant la position actuelle  
- **D-Pad** : un bouton visuel par direction  
- **Triggers** : barres verticales proportionnelles à la pression  
- **Boutons L4 / R4** : zones affichées séparément

---

##  Installation

1. Cloner le dépôt :
   ```bash
   git clone https://github.com/votre-utilisateur/MonogameController.git
