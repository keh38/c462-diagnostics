## Changelog

### v0.17 (unreleased)
#### Changed
- modified gaze calibration to save log, allow data streaming
#### Added 
- manual audiometer

### v0.16.2 (2025-07-28)
#### Fixed
- haptic/electric lateralities were defaulting to diotic, effectively producing crosstalk

---

### v0.16.1 (2025-07-25)
#### Fixed
- Digitimer library was only setting the first device (by ID) 

---

### v0.16 (2025-07-24)
#### Added
- Bekesy-method audiogram

---

### v0.15 (2025-07-23)
#### Added
- Protocol: exposed control of list and instruction font sizes
- Restored Audiogram, LDL, Questionnaires (checklists only)
- Turandot: added option to set parameter-specific screen color

---

### v0.14.1 (2025-07-14)
#### Fixed
- Intercom: play audio source when talk started, to ensure it is running

---

### v0.14 (2025-07-10)
#### Changed
- Server sends audio sync log in response to request to avoid race condition in Turandot where scene change prevented controller from receiving it
- Protocol: added click to continue prompt

---

### v0.13.1 (2025-07-08)
#### Fixed
- apply position to manikin panel
- correctly hide checklist's apply button when specified
- made checklist result into valid JSON

---

### v0.13 (2025-07-08)
#### Changed
- hide cursor during pupil dynamic range test
- made slider start position manikin-specific
#### Fixed 
- Turandot: now applies checklist position

---

### v0.12 (2025-07-02)
#### Added
- Turandot: exposed button and checklist font size
- Pupil Dynamic Range: specification of min and max screen intensity

---

### v0.11 (2025-06-26)
#### Added
- option to hide protocol entry from subject-facing outline
- optional protocol entry instructions
#### Changed
- Manikins:
  - exposed options: text size, start position
  - made fill transparent
- Restored key code option to Turandot buttons
- Checklist:
  - made "auto advance" optional
  - made option to allow multiple selections

---

### v0.10 (2025-06-19)
#### Added
- communication of LED changes to controller
- hide cursor option for Turandot states
- exposed pupil dynamic range parameters
- save screen and LED colors in Turandot data files
#### Changed
- generalize manikin functionality
- Turandot will run with instructions only

---

### v0.9 (2025-06-12)
#### Added
- restored manikins
- Turandot: set LED color (if specified)
- project-specific default (ambient) LED color
#### Changed
- added number of pixels to LED control
#### Fixed
- Launcher traps process errors when stopping/restarting the audio service

---

### v0.8 (2025-05-30)
#### Added
- restored Image cue
- added Video cue
- features to facilitate developing project
  - option to launch in windowed mode
  - use controller's project folder when running on localhost
#### Changed
- use project-specific resource folders

---

### v0.7 (2025-05-13)
#### Added
- intercom
- Turandot: option to apply custom screen color
#### Fixed
- Turandot: restored application of metrics to Turandot sequence

---

### v0.6 (2025-04-24)
#### Added
- Protocols (by remote control)
#### Changed
- Unity 6.0.47f1

---

### v0.5 (2025-04-18)
#### Added
- Turandot: restored slider functionality
- LED control
- Restored metrics

---

### v0.4 (2025-04-11)
#### Added
- gaze calibration (by remote control)
- subject-specific background color

---

### v0.3 (2025-04-10)
#### Added
- pupil dynamic range measurement (by remote control)

---

### v0.2 (2025-04-07)
#### Fixed
- Admin tools: hardware panel selection
- Turandot Interactive: user control of Digitimer demand
#### Changed
- Display list of hardware errors when there issues are detected at startup
- Display status of sync pulse test

---

### v0.1 (2025-04-04)
- initial release for use in C462

