# Implementation

## Vad identifierade agenten?

Den identifierade bra en generell kodstruktur med vettiga beroenden. Även externa beroenden som behövde installeras listade den redan initialt. Den identifierade vad som behöver göras i ett första steg för att få upp en bas med .gitignore och liknande. Vissa features och småsaker som inte direkt stod med i beskrivningen tog den också fram.

## Vad ville agenten göra som du stoppade och varför?

Det enda jag uttryckligen stoppade var att den ville lägga alla interface i ett core-projekt och jag tyckte att de hörde bättre hemma där de används.

## Vad missade den?

Den hade missat lite i vilken ordning några saker borde göras och att en saker behöver göras löpande och inte på slutet. Den valde att använda SQLite för tester men missade att en del skillnader mellan SQLite och SQL Express gör att vissa tester inte säger någonting. Den hade inte med någonting om hur man deployar projektet. I övrigt var det en del saker som att vissa klasser/metoder gjorde för mycket och behövde delas upp.

## Är projektet körbart efter implementationen?

Ja
