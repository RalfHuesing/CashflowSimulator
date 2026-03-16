# TODO: Optimierung Lebensphasen-Modell

## Status
- **Priorität:** Hoch (Architektur-Kern)
- **Status:** Brainstorming / Entwurf
- **Bezug:** [ARCHITECTURE.md](../../ARCHITECTURE.md) & [brainstorming_life_phases.md](../../.gemini/antigravity/brain/b321ae51-0447-4d6c-9e49-8ccf3b872ca4/brainstorming_life_phases.md)

## Problemstellung
Aktuell sind **Lebensphasen** (`LifecyclePhaseDto`) und **Cashflows** (`CashflowStreamDto` / `CashflowEventDto`) entkoppelt. 
- Ein Phasenwechsel (z. B. Rente mit 63 statt 67) erfordert manuelle Anpassungen an allen betroffenen Cashflows.
- Die UI spiegelt den ganzheitlichen Lebensplanungs-Ansatz noch nicht wider (zu viele isolierte Listen).

## Geplante Maßnahmen

### 1. Datenmodell & Engine (Contracts & Engine)
- [ ] **Phase-Anker:** Einführung von optionalen Referenzen in `CashflowStreamDto` und `CashflowEventDto`:
  - `StartLifecyclePhaseId`: Cashflow startet mit dieser Phase.
  - `EndLifecyclePhaseId`: Cashflow endet mit dem Start der Folgephase (oder dieser Phase).
- [ ] **Validierung:** Sicherstellen, dass Phasen-Verknüpfungen konsistent bleiben (keine verwaisten IDs).
- [ ] **Engine-Logik:** `CashflowProcessor` muss die `LifecyclePhaseId` auflösen, falls keine harten Alter/Daten gesetzt sind.

### 2. UI / UX (Desktop)
- [ ] **Timeline-View:** Entwicklung eines zentralen Dashboards mit einer visuellen Zeitachse (Lebensreise).
  - Phasen als Blöcke.
  - Cashflows als darunterliegende Balken.
  - Drag-and-Drop für Phasen-Trenner (automatisches Verschieben der Anker-Cashflows).
- [ ] **Übersichtlichkeit:** Reduzierung der Zersplitterung durch Zusammenfassung von Phasen, Strategien und Allokationen in einer "Lebensplan"-Ansicht.

### 3. Logik-Erweiterungen
- [ ] **Glidepath-Integration:** Vollständige Umsetzung des gleitenden Allokationsübergangs in der Engine (monatliche lineare Interpolation der Gewichte).
- [ ] **Risk-Buckets:** Automatische Generierung von Allokations-Vorschlägen basierend auf der Entnahmedauer der nächsten Phase.

## Offene Fragen
- Sequenzielle vs. parallele Phasen (Sabbatical vs. Erwerbsleben).
- Priorität: Erst die Logik-Verknüpfung (Backend) oder erst die Timeline (Frontend)?
