LoadFromFile %0
CreateTarget %0 %TARGET
SetRead %0
SetWrite %TARGET
ProcessFull %0 %TARGET antistamp_v1.shader.cs 8 32
SetNoAccess %0
SetNoAccess %TARGET
SaveToFile %TARGET %1 png 