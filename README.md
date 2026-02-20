# Tekx
Tekx is a procedural texture generator I coded way back in 2013.

It's based on Worley/Voronoi/cellular noise.

Heavily hardcoded to use the .NET 4.0 platform + Windows Forms specifically.

I wanted to rewrite it to be more architecturally independent, but I don't think that's happening in nearest times.

It reached version 0.8 when I decided to stop its development to make (more) games instead. Some of the code is unused.

Tekx uses a visual interface for everything. Completely undocumented for the moment.

<img width="1317" height="661" alt="456" src="https://github.com/user-attachments/assets/dea95b3e-1d82-44e5-a462-6bb7a5328ae5" />

<img width="1024" height="1024" alt="d92r3n7-f75cf6c9-584b-4ed7-ac54-55a0766d61d3" src="https://github.com/user-attachments/assets/6e6230a9-8535-45ee-af8f-8b23dc25d5c0" />

![9_by_xitilon_d92r3i6-375w-2x](https://github.com/user-attachments/assets/803e60c6-154c-4f01-ab79-d3a27ccd9857)

![waves_by_xitilon_d92qznt-fullview](https://github.com/user-attachments/assets/62d687c1-90cb-4e12-b55f-41e542c48a26)

# MiniTekx

MiniTekx is a mini version of Tekx that I coded in 2015.

It works from a command line, is less feature-rich and convenient, but works faster and could be used to generate textures on the fly as a request from an external application (a game for example).

The arguments are:

width, height, color1, color2, points, iterations, bitcrush, wrap (true or false), facet power

Example:

MiniTekx.exe 512 512 0 16777215 100 3 20 true 10

This will generate a 512x512 texture colored from black to white, with 100 points, 3 iterations, applying 20 of bitcrush effect, wrapping it over the edges (the resulting texture could be tiled seamlessly) and using 10 as a cell facet highlight amount.

<img width="512" height="512" alt="0u599a1a86-7df02116-1cc34859" src="https://github.com/user-attachments/assets/a252f81a-1850-408f-ae61-cb016f1b1b83" /><img width="512" height="512" alt="Minitekx01" src="https://github.com/user-attachments/assets/bd82f7b6-ef4c-4496-ac3f-1bb1cad11fed" />
