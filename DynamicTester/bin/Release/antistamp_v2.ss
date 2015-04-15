LoadFromFile %0
CreateTarget %0 %TARGET
CreateTarget %0 %TARGET2
SetRead %0
SetReadWrite %TARGET
SetWrite %TARGET2
ProcessFull %0 %TARGET antistamp_v1.shader.cs 8 32
ProcessFull %TARGET %TARGET2 sobel.cs 8 32
SetNoAccess %0
SetNoAccess %TARGET
SetNoAccess %TARGET2
SaveToFile %TARGET2 %1 png 