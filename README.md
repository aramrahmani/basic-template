# Basic UI Design

Bootstrap components inspired from material ui.

## Guidelines

#### react-basic-design

    - put bootstrap & react-basic-design styles in file: ./src/index.scss

### Fonts

    - put your font in folder: /public/fonts

    - write css in to html file: /public/index.html
        @font-face {
            font-family: "iranyekan";
            src: url(/fonts/iranyekan/iranyekanwebregular.woff) format("woff"), url(/fonts/iranyekan/iranyekanwebregular.ttf) format("truetype");
            font-style: normal;
            font-weight: normal;
            font-display: swap;
        }

    - assign bootstrap font variable ($font-family-sans-serif) in style file (./src/index.scss)
        $font-family-sans-serif: iranyekan, tahoma, "Segoe UI", Roboto, Arial;
