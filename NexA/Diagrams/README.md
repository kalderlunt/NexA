# Diagrams - NexA Technical Documentation

## 📋 Vue d'ensemble

Ce dossier contient tous les schémas structurés pour générer des diagrammes techniques pour le GDD (Game Design Document) de NexA.

**Objectif**: Fournir des descriptions détaillées à une IA pour générer des diagrammes visuels (PlantUML, Mermaid, Draw.io, etc.).

---

## 📁 Structure des fichiers

| Fichier | Contenu | Diagrammes |
|---------|---------|------------|
| **01_ARCHITECTURE_GLOBALE.md** | Architecture complète du système | 5 diagrammes (1-5) |
| **02_FLUX_UTILISATEUR.md** | Parcours utilisateur et user journeys | 7 diagrammes (6-12) |
| **03_SCHEMA_DATABASE.md** | Schéma PostgreSQL et stratégies data | 7 diagrammes (13-19) |
| **04_BACKEND_API.md** | Architecture backend NestJS | 9 diagrammes (20-28) |
| **05_API_CONTRACTS.md** | Contrats API REST complets | 7 diagrammes (29-35) |
| **06_UNITY_ARCHITECTURE.md** | Architecture client Unity | 7 diagrammes (36-42) |
| **07_DESIGN_SYSTEM.md** | Design system & animations UI | 10 diagrammes (43-52) |
| **08_INFRASTRUCTURE_OPS.md** | Infrastructure Debian & observabilité | 7 diagrammes (53-59) |
| **09_ROADMAP_MVP.md** | Roadmap, MVP, risques, métriques | 9 diagrammes (60-68) |

**Total**: 68 diagrammes structurés

---

## 🎯 Comment utiliser ces fichiers

### Étape 1: Choisir les diagrammes nécessaires

Parcourez les fichiers et identifiez les diagrammes pertinents pour votre GDD.

### Étape 2: Extraire les descriptions

Copiez les sections de diagrammes (entre les titres `## Diagramme X:`) vers votre outil de génération.

### Étape 3: Générer les diagrammes

Utilisez une IA (GPT-4, Claude, etc.) ou un outil pour générer:

**Outils recommandés**:
- **PlantUML**: Diagrammes UML textuels
- **Mermaid**: Diagrammes markdown (GitHub support)
- **Draw.io**: Diagrammes visuels
- **Lucidchart**: Diagrammes professionnels
- **Excalidraw**: Sketches rapides
- **Figma**: Mockups UI

**Prompt exemple pour IA**:
```
Generate a PlantUML diagram based on this specification:
[Copier la description du diagramme]

Output format: PlantUML code
```

### Étape 4: Intégrer au GDD

Exportez les diagrammes en PNG/SVG et intégrez-les dans votre document technique.

---

## 📊 Index des diagrammes par catégorie

### Architecture & Infrastructure (17 diagrammes)

| # | Nom | Fichier | Type |
|---|-----|---------|------|
| 1 | Architecture 3-Tiers | 01 | Architecture diagram |
| 2 | Architecture Client Unity (Layers) | 01 | Component diagram |
| 3 | Architecture Backend (NestJS Modules) | 01 | Module diagram |
| 4 | Infrastructure Debian (Deployment) | 01 | Deployment diagram |
| 5 | Flux réseau et sécurité | 01 | Network flow |
| 20 | Structure Modulaire NestJS | 04 | Module dependency |
| 21 | Flux de Requête HTTP (Pipeline) | 04 | Sequence diagram |
| 22 | Architecture en Couches | 04 | Layered architecture |
| 36 | Structure de Dossiers Unity | 06 | Folder tree |
| 53 | Déploiement Debian | 08 | Infrastructure diagram |
| 54 | Nginx Configuration | 08 | Config diagram |
| 55 | Systemd Services | 08 | Service config |
| 56 | Grafana Dashboards | 08 | Dashboard layout |
| 57 | Alerting Rules | 08 | Alert config |
| 58 | Backup Strategy | 08 | Backup flow |
| 59 | Deployment Checklist | 08 | Checklist |

---

### User Flows & Sequences (12 diagrammes)

| # | Nom | Fichier | Type |
|---|-----|---------|------|
| 6 | Flow Chart - Parcours Complet | 02 | Flowchart |
| 7 | Séquence - Authentification (Login) | 02 | Sequence diagram |
| 8 | Séquence - Chargement Liste d'Amis | 02 | Sequence diagram |
| 9 | Séquence - Envoi Demande d'Ami | 02 | Sequence diagram |
| 10 | État des Écrans (State Machine) | 02 | State machine |
| 11 | User Journey Map - Première Session | 02 | Journey map |
| 12 | Flow d'erreur et retry logic | 02 | Flowchart |
| 25 | Authentification Flow (JWT) | 04 | Sequence diagram |
| 38 | State Machine - Navigation UI | 06 | State machine |
| 40 | Network Layer - Retry Logic | 06 | Flowchart + sequence |
| 47 | Screen Transitions | 07 | Transition flow |

---

### Database & Data (7 diagrammes)

| # | Nom | Fichier | Type |
|---|-----|---------|------|
| 13 | ERD (Entity Relationship Diagram) | 03 | ERD |
| 14 | Stratégie d'Index et Performances | 03 | Index strategy |
| 15 | Stratégie de Migration | 03 | Timeline |
| 16 | Data Flow - Création de Match | 03 | Data flow |
| 17 | Archivage et Rétention | 03 | Flowchart |
| 18 | Gestion des Sessions et Tokens | 03 | Data model |
| 19 | Stratégie de Backup | 03 | Infrastructure |

---

### API & Contracts (7 diagrammes)

| # | Nom | Fichier | Type |
|---|-----|---------|------|
| 29 | API Endpoints Map | 05 | API map |
| 30 | API Contract - Auth Module | 05 | API documentation |
| 31 | API Contract - Users Module | 05 | API documentation |
| 32 | API Contract - Friends Module | 05 | API documentation |
| 33 | API Contract - Matches Module | 05 | API documentation |
| 34 | Codes d'Erreur Complets | 05 | Error matrix |
| 35 | Headers HTTP Standard | 05 | HTTP spec |

---

### Backend Techniques (8 diagrammes)

| # | Nom | Fichier | Type |
|---|-----|---------|------|
| 23 | Gestion des Erreurs | 04 | Class hierarchy + flow |
| 24 | Rate Limiting Strategy | 04 | Component diagram |
| 26 | Structure de Logs JSON | 04 | Schema + examples |
| 27 | Métriques Prometheus | 04 | Metrics spec |
| 28 | Configuration Environment Variables | 04 | Config schema |

---

### Unity Client (7 diagrammes)

| # | Nom | Fichier | Type |
|---|-----|---------|------|
| 37 | Pattern Singleton pour Services | 06 | Class diagram |
| 39 | BaseScreen Architecture | 06 | Class hierarchy |
| 41 | Cache Strategy | 06 | Component + flow |
| 42 | Animation Service (DOTween) | 06 | Utility class |

---

### Design System (10 diagrammes)

| # | Nom | Fichier | Type |
|---|-----|---------|------|
| 43 | Design System - Palette de Couleurs | 07 | Color palette |
| 44 | Typography System | 07 | Typography guide |
| 45 | Component Library | 07 | Component showcase |
| 46 | Animation Timing & Easing | 07 | Animation timeline |
| 48 | Loading States | 07 | UI state diagram |
| 49 | Toast Notifications | 07 | Component spec |
| 50 | Hover & Focus States | 07 | Interaction states |
| 51 | Responsive Layout | 07 | Layout grid |
| 52 | Iconography | 07 | Icon library |

---

### Roadmap & Planning (9 diagrammes)

| # | Nom | Fichier | Type |
|---|-----|---------|------|
| 60 | MVP Scope | 09 | Feature matrix |
| 61 | Roadmap 3 Phases | 09 | Gantt chart |
| 62 | Dependencies Graph | 09 | Dependency graph |
| 63 | Risk Matrix | 09 | Risk assessment |
| 64 | Team Structure | 09 | Org chart |
| 65 | Success Metrics | 09 | KPI dashboard |
| 66 | Decision Log | 09 | Decision record |
| 67 | Testing Strategy | 09 | Test pyramid |
| 68 | Learning Checklist | 09 | Skills matrix |

---

## 🎨 Palette de couleurs globale

Utilisez ces couleurs pour cohérence visuelle:

### Par composant
```
Client Unity:       #00C853 (vert)
Backend API:        #2196F3 (bleu)
Database:           #FF9800 (orange)
Observability:      #9C27B0 (violet)
Infrastructure:     #607D8B (gris bleu)
```

### Par layer (Unity)
```
Core:               #F44336 (rouge)
Network:            #2196F3 (bleu)
Services:           #4CAF50 (vert)
UI:                 #FF9800 (orange)
Models:             #9C27B0 (violet)
```

### Design system
```
Primary Gold:       #C8AA6E
Background Dark:    #010A13
Text Primary:       #F0E6D2
Success:            #00C853
Error:              #D32F2F
```

---

## 📝 Templates de prompts pour IA

### Pour PlantUML

```
Generate a PlantUML diagram with the following specifications:

Title: [Titre du diagramme]
Type: [Architecture/Sequence/Class/State/etc.]

Components:
[Copier la section "Composants à représenter"]

Relationships:
[Copier la section "Connexions" ou "Dépendances"]

Style: Use color #2196F3 for backend, #00C853 for client

Output: Complete PlantUML code ready to render
```

### Pour Mermaid

```
Generate a Mermaid diagram (for GitHub markdown) with these specs:

[Copier la description du diagramme]

Output format: Mermaid markdown code block
```

### Pour Draw.io / Lucidchart

```
Describe step-by-step how to create this diagram in Draw.io:

[Copier la description]

Include: shapes to use, layout suggestions, color codes
```

---

## 🔧 Outils de génération automatique

### PlantUML CLI

```bash
# Installer PlantUML
brew install plantuml  # macOS
apt install plantuml   # Debian

# Générer PNG
plantuml diagram.puml

# Générer SVG
plantuml -tsvg diagram.puml
```

### Mermaid CLI

```bash
# Installer
npm install -g @mermaid-js/mermaid-cli

# Générer
mmdc -i diagram.mmd -o diagram.png
```

### Python + Graphviz

```python
from graphviz import Digraph

dot = Digraph(comment='NexA Architecture')
dot.node('A', 'Client Unity')
dot.node('B', 'Backend API')
dot.edge('A', 'B', 'HTTPS')

dot.render('architecture.gv', view=True)
```

---

## 📦 Export pour GDD

### Format recommandé

**Pour chaque diagramme**:
1. **Titre**: Nom du diagramme
2. **Description**: 2-3 phrases de contexte
3. **Image**: PNG haute résolution (300 DPI) ou SVG
4. **Légende**: Explication des symboles/couleurs
5. **Notes**: Points importants à retenir

### Structure GDD suggérée

```markdown
# Architecture Technique - NexA

## 1. Vue d'ensemble
[Diagramme 1: Architecture 3-Tiers]

## 2. Backend
### 2.1 Structure modulaire
[Diagramme 20: Structure NestJS]

### 2.2 API Endpoints
[Diagramme 29: API Map]

## 3. Client Unity
[Diagrammes 36-42]

## 4. Infrastructure
[Diagrammes 53-59]

## 5. Roadmap
[Diagrammes 60-68]
```

---

## 🚀 Quick Start

**Pour générer tous les diagrammes essentiels pour un MVP**:

1. Architecture: Diagrammes 1, 2, 3
2. User Flows: Diagrammes 6, 7
3. Database: Diagramme 13
4. API: Diagramme 29
5. Unity: Diagrammes 36, 38
6. Roadmap: Diagramme 60

**Total**: 10 diagrammes couvrent 80% des besoins MVP.

---

## 💡 Conseils

### Pour les présentations
- Utilisez SVG (scalable) pour les slides
- Animations progressives (révéler composants un par un)
- Palette cohérente (stick to design system colors)

### Pour la documentation
- PNG haute résolution (1920x1080 minimum)
- Légendes claires
- Liens vers code source si applicable

### Pour le développement
- Imprimez les diagrammes clés (architecture, data flow)
- Épinglez-les près de votre bureau
- Référez-vous lors du code review

---

## 📚 Ressources externes

### Apprendre les diagrammes
- [C4 Model](https://c4model.com/) - Architecture diagrams
- [PlantUML Guide](https://plantuml.com/guide) - Syntax reference
- [Mermaid Live Editor](https://mermaid.live/) - Online editor

### Inspiration
- [System Design Primer](https://github.com/donnemartin/system-design-primer)
- [AWS Architecture Icons](https://aws.amazon.com/architecture/icons/)
- [Azure Architecture](https://docs.microsoft.com/azure/architecture/)

### Outils collaboratifs
- [Miro](https://miro.com/) - Brainstorming
- [FigJam](https://www.figma.com/figjam/) - Collaborative whiteboard
- [Excalidraw](https://excalidraw.com/) - Hand-drawn diagrams

---

## 🤝 Contribution

Si vous ajoutez de nouveaux diagrammes:

1. Numérotez-les séquentiellement (69, 70, etc.)
2. Suivez le format existant (Type, Description, Specs)
3. Ajoutez les métadonnées (couleurs, outils)
4. Mettez à jour cet index

---

## 📞 Support

Pour questions ou suggestions sur ces schémas:
- Créez une issue GitHub
- Contactez l'architecte projet
- Consultez la documentation complète dans `/MDHelp/`

---

**Version**: 1.0
**Dernière mise à jour**: 2026-01-11
**Auteur**: Lead Architect NexA
**License**: Internal use only


