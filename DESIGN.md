---
name: SCHLAFWALD
description: A quiet storybook clearing with soft three-dimensional animals and a restrained bedtime interface.
colors:
  night-teal: "#182e37"
  moon-cream: "#f4e8cf"
  moss-island: "#687d55"
  leaf-sage: "#91aa72"
  peach-skin: "#e9cdb1"
  shell-rose: "#d29c93"
  action-peach: "#ecd4ad"
  action-ink: "#29423b"
  panel: "#f4efdf"
  ink: "#28433f"
  adult-action: "#49654e"
  adult-action-label: "#fbf2de"
  secondary-surface: "#e2e4cf"
  secondary-ink: "#344e40"
  adult-body: "#51665a"
  species-label: "#586b52"
  field-surface: "#fbf8ec"
  field-ink: "#365440"
  field-border: "#b9c5ae"
  divider: "#d6dcca"
  form-accent: "#516e54"
  focus-ochre: "#a97947"
typography:
  display:
    fontFamily: "Lora, Georgia, serif"
    fontSize: "clamp(30px, 3.6vw, 54px)"
    fontWeight: 400
    lineHeight: 1.13
    letterSpacing: "-0.035em"
  animal-title:
    fontFamily: "Lora, Georgia, serif"
    fontSize: "clamp(28px, 4vw, 48px)"
    lineHeight: 1.2
  headline:
    fontFamily: "Lora, Georgia, serif"
    fontSize: "30px"
    fontWeight: 400
    lineHeight: 1.18
    letterSpacing: "-0.025em"
  title:
    fontFamily: "Schlafwald, 'Trebuchet MS', sans-serif"
    fontSize: "17px"
  body:
    fontFamily: "Schlafwald, 'Trebuchet MS', sans-serif"
    fontSize: "clamp(14px, 1.35vw, 18px)"
    lineHeight: 1.6
  narration:
    fontFamily: "Schlafwald, 'Trebuchet MS', sans-serif"
    fontSize: "clamp(14px, 1.65vw, 21px)"
    fontWeight: 600
    lineHeight: 1.55
  adult-body:
    fontFamily: "Schlafwald, 'Trebuchet MS', sans-serif"
    fontSize: "14px"
    lineHeight: 1.6
  action:
    fontFamily: "Schlafwald, 'Trebuchet MS', sans-serif"
    fontSize: "15px"
    fontWeight: 800
  secondary-action:
    fontFamily: "Schlafwald, 'Trebuchet MS', sans-serif"
    fontSize: "14px"
    fontWeight: 700
  wordmark:
    fontFamily: "Schlafwald, 'Trebuchet MS', sans-serif"
    fontSize: "13px"
    fontWeight: 800
    letterSpacing: "0.20em"
rounded:
  field: "10px"
  secondary: "12px"
  dialog: "20px"
  capsule: "24px"
  primary: "27px"
  circle: "50%"
spacing:
  compact: "8px"
  small: "12px"
  medium: "16px"
  roomy: "20px"
  section: "24px"
  panel-inline: "32px"
  dialog-inset: "40px"
components:
  button-primary:
    backgroundColor: "{colors.action-peach}"
    textColor: "{colors.action-ink}"
    typography: "{typography.action}"
    rounded: "{rounded.primary}"
    padding: "14px 23px"
  button-adult-primary:
    backgroundColor: "{colors.adult-action}"
    textColor: "{colors.adult-action-label}"
    rounded: "{rounded.primary}"
    padding: "14px 23px"
  button-secondary:
    backgroundColor: "{colors.secondary-surface}"
    textColor: "{colors.secondary-ink}"
    typography: "{typography.secondary-action}"
    rounded: "{rounded.secondary}"
    padding: "12px 18px"
  button-quiet:
    backgroundColor: "transparent"
    textColor: "{colors.moon-cream}"
    padding: "12px"
  parent-access:
    backgroundColor: "rgba(26,47,45,.63)"
    textColor: "#e5e1ce"
    rounded: "{rounded.capsule}"
    padding: "10px 16px"
  select:
    backgroundColor: "{colors.field-surface}"
    textColor: "{colors.field-ink}"
    rounded: "{rounded.field}"
    padding: "10px 14px"
    width: "100%"
  adult-dialog:
    backgroundColor: "{colors.panel}"
    textColor: "{colors.ink}"
    rounded: "{rounded.dialog}"
    padding: "0"
    width: "min(780px, calc(100vw - 40px))"
  gesture:
    backgroundColor: "rgba(32,55,47,.88)"
    textColor: "#e8e8cf"
    rounded: "{rounded.capsule}"
    padding: "10px 16px"
  journey:
    textColor: "#bcc9b9"
  checkbox-row:
    textColor: "{colors.ink}"
---

# Design System: SCHLAFWALD

## Overview

**Creative North Star: "The quiet storybook clearing"**

The world is a soft, genuine three-dimensional forest at dusk. Rounded animal bodies, cream faces, rose and peach details, moss islands, overlapping trees and small points of warm light create the children's storybook character. The forest carries the experience; the interface stays sparse and leaves the animals visible at a child's eye level.

Warm display lettering and rounded body text keep the small amount of language gentle. Adult controls occupy a separate cream dialog with dark green text, native form fields and clear keyboard focus. This records the implemented browser world in `WebApp/src/style.css`, `world.js` and `main.js`, checked against the final welcome and parent captures; it is not a replacement direction for the retained Unity project.

**Key Characteristics:**

- A continuous forest image with restrained text overlays.
- Soft mesh silhouettes with distinct animal anatomy and sleep gestures.
- Cream, moss, peach and rose held against a cool evening atmosphere.
- Large, rounded actions and a protected, readable adult dialog.

## Colors

Warm, low-saturation surfaces sit within a cooler forest atmosphere; frontmatter values are the normative source colors, while scene lighting changes their rendered appearance. The sidecar's synthesized tonal strips are preview metadata, not additional material tokens.

### Primary

- **Moss Island** and **Leaf Sage:** the sleep-place ground and Suri's large leaf shelter. The forest also uses layered greens rather than a single flat fill.
- **Action Peach** with **Action Ink:** the welcoming child-facing action, warm against the darker scene.

### Secondary

- **Peach Skin** and **Shell Rose:** Suri's face and shell; related warm materials distinguish other animals, mushrooms and bedding without turning them into interface accents.
- **Adult Action** with **Adult Action Label:** the return-to-forest action on the pale adult surface.

### Neutral

- **Night Teal:** the page's dark base beneath the rendered forest.
- **Moon Cream:** default text over the forest and the quiet continuation action.
- **Panel**, **Ink** and **Adult Body:** the adult dialog surface, primary text and explanatory text.
- **Species Label:** readable secondary animal descriptions in the adult selection grid.
- **Secondary Surface** and **Secondary Ink:** adult utility actions and arithmetic choices.
- **Field Surface**, **Field Ink**, **Field Border** and **Form Accent:** native selects, checkboxes and range controls.
- **Divider:** thin structural separators inside the adult dialog. **Focus Ochre:** the keyboard focus outline.

**The Forest First Rule.** Use scene materials to establish the world; reserve interface color for readable text, gentle actions and adult controls.

## Typography

**Display Font:** self-hosted Lora, with Georgia and serif fallbacks. **Body Font:** self-hosted Nunito, declared locally as `Schlafwald`, with Trebuchet MS and sans-serif fallbacks. There is no separate mono face.

The display face supplies a storybook voice; the round body face stays direct and readable. Both font files are variable: Lora is declared from weights 400–700 and Nunito from 400–800. The scale is role-specific, not a fixed modular ratio.

### Hierarchy

- **Display:** the frontmatter display role is used for welcome and completion headings, with balanced wrapping.
- **Animal title:** a fluid serif name above the smaller species line.
- **Headline:** the adult dialog heading. Settings section titles use the smaller body-family title role.
- **Body:** fluid welcome copy; adult explanations use the adult-body role. Adult notes and species descriptions step down to 12px and 11px respectively.
- **Narration:** centered, semibold subtitles with a 64ch maximum line length and layered text shadows.
- **Action:** bold labels on the peach primary. Adult secondary actions use their own smaller, lighter role.
- **Wordmark:** the letter-spaced uppercase brand name in the persistent header. This is the brand signature, not a pattern for introducing sections.

**The Story Voice Rule.** Use Lora for story and dialog headings and Nunito for controls, body copy and labels; keep the welcome hierarchy beginning with its heading.

## Layout

The application fills the viewport and does not scroll at page level. A Three.js canvas provides the background; fixed overlays create the interface. The header respects horizontal safe-area insets with a minimum 24px margin. The welcome occupies a lower-left text area, with a 560px maximum width, fluid left inset and fluid bottom inset. During a ritual, the chapter name is upper-left, narration is centered near the bottom, the gesture capsule sits above it and the seven-animal journey sits at the bottom center.

Spacing follows observed small gaps and roomier section insets from the frontmatter. Primary controls are at least 52px high on the regular layout; quiet, secondary, select, close and parent-access controls are at least 48px. Native checkboxes are at least 24px inside larger label rows. The animal grid has three columns, 8px row gaps and 20px column gaps. The adult dialog has a fixed header and footer around its independently scrolling settings area; its sections use 24px vertical padding and 32px horizontal container insets. Smaller gate and confirmation dialogs are at most 460px wide with 40px padding.

Landscape is the intended play composition. At landscape heights of 500px or less, the header moves to a 12px top inset, the welcome narrows to 390px, the main heading becomes 32px, copy becomes 13px, and primary actions use 48px minimum height with 11px 19px padding. Adult dialog insets and headings also compact. In portrait, the welcome becomes a full-width lower overlay with a 39px heading, the animal grid becomes two columns, parent utility actions stack, and a quiet landscape invitation appears at the bottom. The portrait camera repositions the welcome animal so its face and shell stay visible above the text. The camera uses a 52-degree field of view below a 1.5 aspect ratio and 42 degrees otherwise.

## Elevation & Depth

Depth comes chiefly from actual meshes, overlapping trunks and canopies, foreground ferns, moss islands, directional light and fog. Materials are predominantly rough and softly shaded. The implementation uses ACES filmic tone mapping, a warm key light and a cooler rim light, without postprocessing bloom or hard real-time shadows. Transparent edge gradients improve overlay readability; the adult dialog uses a dark translucent backdrop.

### Shadow Vocabulary

- **Primary action:** a diffuse low shadow (`0 8px 22px rgba(16,35,32,.16)`) below the peach and adult primary actions.
- **Adult dialog:** a broad ambient shadow (`0 24px 90px rgba(8,25,23,.3)`) separating the cream surface from the forest.
- **Display text:** a soft shadow (`0 2px 24px rgba(12,28,28,.25)`) on welcome and completion headings.
- **Narration:** paired shadows (`0 2px 9px #122b2c, 0 1px 24px #122b2c`) keeping optional subtitles legible over the scene.
- **Current journey dot:** a small warm glow (`0 0 14px rgba(241,214,159,.5)`) identifying the current animal.

**The Soft Depth Rule.** Use overlapping geometry, tonal separation and diffuse shadows; retain the rounded, calm material character of the forest.

Scene fades use a one-second ease. Adult access displays a three-second linear hold fill. Buttons brighten slightly on hover and move down one pixel while pressed. Animal motion eases toward poses, with slow breathing, blinking and small head and tail movements; these are scene behaviors, not decorative UI animation. The reduced-motion media rule removes the CSS scene fade and forces automatic scroll behavior; it does not currently disable the Three.js animal motion.

## Shapes

Actions are capsules, secondary adult actions are softly rounded rectangles, and dialogs have broad rounded corners. Fields use the tighter field radius. The same system permits thin adult separators and field borders; they provide structure on cream rather than decoration over the forest. Circular close targets, small journey dots and fine interaction halos supply the round details.

The forest uses real rounded and extruded geometry: curved leaf surfaces with visible veins, padded draped cloth, closed shell volumes and a raised spiral. Distinct ear, muzzle, limb and tail shapes make each animal recognizable. Interface icons are simple inline SVG strokes with round caps and joins, not text glyph substitutes.

## Components

### Buttons

Friendly, restrained actions. The primary uses the peach and dark ink pair, the primary radius, 14px 23px padding and a 20px icon gap. The adult return action keeps that shape with the adult green color pair and a 14px label. Secondary actions use their dedicated color pair, radius and 12px 18px padding. The quiet continuation action is transparent and underlined with a 5px underline offset.

All buttons brighten to 1.05 on hover, move down 1px when pressed and show the shared 3px ochre focus outline at a 4px offset when keyboard-focused. Disabled buttons use 0.45 opacity and a default cursor. Close controls are transparent circular 48px targets.

### Cards / Containers

The adult modal is the principal surface; the child-facing world does not use a card grid. The larger settings surface uses the adult-dialog component values, a broad ambient shadow and thin internal separators. Its header uses 26px 32px 18px padding, the footer uses 16px 32px 22px, and the body scrolls between them. Gate and confirmation surfaces share the panel, ink, radius and shadow with centered content.

### Inputs / Fields

Quiet native form controls. Selects fill their container, have a 48px minimum height, the field color pair, a one-pixel field border and the field radius. Checkboxes and ranges use the form accent. Range controls have a 44px-high interaction area; toggle labels have a 56px minimum row height, while animal labels have 66px. Associated labels and tabular numeric outputs remain part of each control. Focus uses the shared ochre outline.

### Navigation

The journey is a passive progress strip, not a row of navigation buttons. Each animal has a 6px circular dot above an 11px name, separated by 24px. The current animal gains warm text and a glowing warm dot; sleeping animals use a softer green dot. The landscape compact and portrait layouts tighten the strip as described in Layout. The header keeps only the brand and protected adult access.

### Gesture Capsule

A short leaf-icon instruction floats above the narration. It uses a translucent deep green background, pale text, capsule corners and 10px 16px padding. It does not intercept pointer input. Its maximum width is 75vw in the regular layout and 85vw in portrait.

### Protected Adult Access

A quiet translucent outlined capsule in the header uses a 16px lock icon and a 13px label. Holding fills it gently from left to right before opening the cream arithmetic gate. Pointer and keyboard holds share the same three-second duration. The native dialog contains focus, and closing returns focus to the previously active control.

## Do's and Don'ts

### Do:

- **Do** keep the animal and its sleep-place visible beneath sparse overlays.
- **Do** use self-hosted Lora headings and Nunito body text with the recorded roles.
- **Do** preserve the current readable species-label color on the cream adult panel.
- **Do** keep large action targets, visible keyboard focus and the adult dialog's native form controls.
- **Do** use softly curved meshes and distinct anatomy when extending the animal world.

### Don't:

- **Don't** add an eyebrow above the welcome heading or copy the letter-spaced wordmark into section labels.
- **Don't** replace the live three-dimensional forest with a flat concept image or billboard animals.
- **Don't** introduce bloom, hard real-time shadows or sharp bedding into this soft scene language.
- **Don't** turn the passive journey strip into a score, reward meter or competitive progress treatment.
- **Don't** treat the CSS reduced-motion rule as proof that scene motion or physical-device behavior has been validated.
