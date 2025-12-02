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


## 🧪 Tests unitaires

Trois tests unitaires simples.

### StickConfig_InsideDeadZone_ReturnsZero
Ce test vérifie que lorsque la valeur d’un stick est plus petite que la deadzone définie, la méthode `Apply()` retourne bien 0.  
Comme ca on est sûr que la zone morte est correctement appliquée.

### Profile_Constructor_CreatesEmptyList
Ce test vérifie que lorsqu’on crée un nouvel objet `Profile`, la liste des sticks est bien initialisée et vide.  
Cela évite les erreurs dans la gestion du profil et garantit que l’objet commence dans un état propre.

### InputEvent_Constructor_StoresDateAndText
Ce test vérifie que la classe `InputEvent` enregistre correctement la date et le texte qui lui sont transmis.  
Cela garantit que l’affichage de l’historique contient les bonnes info.

---



