# MonoGameUniversalEffects

This project is based on https://github.com/infinitespace-studios/InfinitespaceStudios.Pipeline
All code is under the MIT licence. 

## RemoteEffectProcessor

This processor is designed to allow Mac OS and Linux developers compile shaders on their platforms. It does this by 
using a remote service on a Vagrant box to compile the shader.


### Usage

1. Download the latest version of client.zip from the [Releases](https://github.com/augustozanellato/MonoGameUniversalEffects/releases) page
2. Extract it somewhere
3. Run ```vagrant up``` in the directory where you extracted the archive (the first time this step could take up to an hour).
4. Meanwhile open your MonoGame Content Pipeline project (the .mgcb file which usually sits under /Content/)
5. Click on the "Content" element under the section "Project"
6. Click on "References" which is the last voice under the "Properties" section
7. Click on "Add" and select ```MonoGameUniversalEffects.Pipeline.dll``` from the directory ```Pipeline Extension``` which was contained the client.zip you extracted earlier.
8. For each .fx file you want to compile with MonoGameUniversalEffects check that its Processor is set to ```Remote Effects Processor - MonoGameUniversalEffects```.
9. Enjoy!
