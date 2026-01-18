# Projekt: TypeAssist

**LLM-gestützte Autovervollständigung mit Travel-Distance-Analyse**

## 1. Repository

Der vollständige Quellcode des Projekts ist unter folgendem Link verfügbar: **URL:** [https://github.com/thzeMedic/TypeAssist](https://github.com/thzeMedic/TypeAssist)

---

## 2. Projektbeschreibung

TypeAssist ist eine Windows-Applikation (WPF), die systemweit Tastenanschläge erfasst und mithilfe eines Large Language Models (LLM) kontextbasierte Wortvorschläge generiert.

Die Softwarelösung ist im Rahmen des Nest Projekts der HCW (Hochschuhle Campus Wien) entstanden und diente im Konzept der Unterstützung von Menschen mit physischen Einschränkungen beim Umgang mit Eingabegeräten (Tastatur) am Computer.

Die Basis des Projekts wurde sich zu nutze gemacht um es im Rahmen der Softwareengineering VL (3. Semster @ CSDC @ HCW) als Projektarbeit zu verwenden - daher ist die Implementierung kein Vollwertiges Produkt das Menschen mit Behinderung optimal helfen kann.

Die Software dient einem MVP im Rahmen der im SRS Dokument spezifizierten MoSCoW requirements.

---

## 3. Installation & Nutzung

### Voraussetzungen

- Windows 10 oder 11
- .NET Runtime (entsprechend der Solution-Version)
- [Ollama](https://ollama.com/) (für lokale Vorhersagen)

### Schritt-für-Schritt Anleitung

1. **Repository klonen** Laden Sie das Repository herunter oder klonen Sie es via Git:
   ```
   git clone https://github.com/thzeMedic/TypeAssist
   ```
2. **Abhängigkeiten installieren** Stellen Sie sicher, dass alle .NET Pakete wiederhergestellt sind (Restore NuGet Packages beim Build).
3. **Lokales LLM einrichten (Ollama)** Installieren Sie Ollama und führen Sie folgenden Befehl in der Konsole aus, um das benötigte Modell zu laden:

   ```
   ollama pull gwen2.5:0.5b
   ```

   _(Dieses Modell wird für den lokalen Autocomplete-Modus verwendet)._

4. **Remote LLM einrichten (Optional)** Für die Nutzung der OpenAI API muss eine Umgebungsvariable unter Windows gesetzt werden:

   ```
   setx OPENAI_API_KEY "ihr-api-key-hier"
   ```

5. **Starten** Kompilieren und starten Sie die `TypeAssist.exe`. Das Overlay erscheint automatisch beim Tippen in anderen Anwendungen.

   - **Auswahl:** Bestätigen Sie einen Vorschlag mit der `Tab`-Taste.
   - **Einstellungen:** Über das Tray-Icon oder das Menü können Modi (Wörter, Silben, Buchstaben) konfiguriert werden.

---

## 4. Anforderungskriterien (MoSCoW)

### Muss-Kriterien (Must Have)

- **M1)** Das System muss unter Windows 10/11 laufen.
- **M2)** Das System muss Tastenanschläge systemweit erfassen, ohne die aktive Anwendung zu blockieren.
- **M3)** Das System muss ein Overlay-Fenster ("GhostWindow") bereitstellen, das visuell über anderen Fenstern liegt.
- **M4)** Das Overlay-Fenster soll Mauseingaben nicht abfangen und an die darunterliegende Applikation durchlassen.
- **M5)** Das System muss Wortvorschläge basierend auf einem lokalen LLM (z.B. Ollama) generieren.
- **M6)** Die Auswahl eines Vorschlags muss über eine definierte Tastatur-Taste (z.B. “Tab-Taste”) erfolgen.
- **M7)** Die nähere Auswahl soll durch das Overlay visuell dargestellt werden und zur Auswahl zur Verfügung gestellt werden. Nach Auswahl müssen die fehlenden Tastenschläge auf der Applikation unterhalb des Overlays emuliert werden.
- **M8)** Der Standardmodus soll (gemäß 5.1) implementiert werden, ohne die Travel Distance zu berücksichtigen.

### Soll-Kriterien (Should Have)

- **S1)** Das System soll die physische Distanz zwischen dem letzten getippten Zeichen und den möglichen nächsten Buchstaben berechnen (“Travel-Distance”).
- **S2)** Der Standardmodus soll die Travel Distance berücksichtigen und Vorschläge unterdrücken, wenn die berechnete Distanz unter einem Schwellenwert liegt.
- **S3)** Das Overlay soll dynamisch in der Nähe des Text-Cursors ODER an einer fixen Bildschirmposition positioniert werden.

### Kann-Kriterien (Could Have)

- **C1)** Die restlichen Modi (Silben, Buchstaben) können nach 5.1 implementiert werden.
- **C2)** Es können verschiedene Eingabegeräte für die Auswahl der Vorhersagen unterstützt werden.
- **C3)** Anzeigeort der Vorschläge können sich konfigurieren lassen.
- **C4)** Die Vorhersagen können auch in einem zusätzlichen Fenster angezeigt werden.
- **C5)** Ein dediziertes Settings-Window kann dem User mögliche Einstellungen zur Verfügung stellen.

### Abgrenzungs-Kriterien (Won't Have)

- **W1)** Es soll kein vollwertiges Autocomplete werden (Fokus liegt auf Assistenz, nicht Fehlerkorrektur).
- **W2)** Es soll nur für Windows zur Verfügung gestellt werden.
- **W3)** Keine Unterstützung für spezielle Hardware (externe Displays, Pedale) im MVP.

---

## 5. Implementierte Modi (Referenz 5.1)

Das System unterstützt folgende über die Einstellungen konfigurierbare Modi:

1. **Ganze Wörter (Standard-Modus):** Das LLM nimmt Tastenschläge auf und schlägt basierend auf dem Kontext ganze Wörter vor. Im Smart-Modus werden diese nur angezeigt, wenn sie über dem Travel-Distance-Threshold liegen.
2. **Silben vorhersagen (Silben-Modus):** Das LLM sagt die nächste logische Silbe des aktuellen Wortes vorher. Vorschläge werden priorisiert, wenn die zu tippende Silbe eine hohe Travel Distance aufweist.
3. **Einzelne Buchstaben (Speed-Modus):** Das LLM sagt den nächsten einzelnen Buchstaben vorher. Dieser Modus ist für schnelles Tippen gedacht, bei dem nur schwer erreichbare Tasten (hohe Travel Distance) vorgeschlagen werden sollen.
