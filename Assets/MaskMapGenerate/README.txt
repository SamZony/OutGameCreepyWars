Mask Map Generator
Mask Map Generator is a powerful tool for Unity that allows you to generate mask maps for your materials. It supports both Metallic and Roughness workflows, providing flexibility for a wide range of materials in your projects.

Features
Generate mask maps based on input textures such as Diffuse, Normal, Metallic, Roughness, and Ambient Occlusion maps.
Choose between Metallic or Roughness workflow using a convenient dropdown menu.
Fine-tune the Roughness value using a slider for greater control over materials with Roughness maps.
Automatic validation to ensure that all input maps have the same dimensions before generating the mask map.
Option to overwrite existing mask maps when saving.

How to Use
After importing the package, go to the "Window" menu and select "Mask Map Generator" to open the Mask Map Generator window.
In the window, you can set the name of the texture that will be saved as the Mask Map.
Choose the type of mask map you want to generate - Metallic or Roughness - from the dropdown menu.
Depending on your selection, you will need to provide the following input maps:
Diffuse Map
Normal Map
Metallic Map (for Metallic workflow) OR Roughness Map (for Roughness workflow)
Ambient Occlusion Map
For the Roughness workflow, you can adjust the Roughness value using the slider provided.
Click the "Generate Mask Map" button to generate the mask map based on your inputs. A preview of the generated mask map will be shown in the window.
If you are satisfied with the result, click the "Save Mask Map" button to save the generated mask map as a PNG file.
If a mask map with the same name already exists, a prompt will ask if you want to overwrite it. Choose "Yes" to proceed with overwriting or "No" to cancel the save operation.
Tips
For Metallic workflow, the Mask Map will take the Red channel of the Metallic map and use it as the Metallic value in the mask.
For Roughness workflow, the Mask Map will take the Red channel of the Roughness map and use it as the Roughness value in the mask. You can fine-tune this value using the Roughness slider.

Requirements
Unity 2019.4 or later
High Definition Render Pipeline (HDRP)