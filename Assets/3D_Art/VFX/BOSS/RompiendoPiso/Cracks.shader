Shader "Masked/Mask" {
    SubShader{
        Tags{"Queue" = "Geometry-1" }

        Cull Off
        ColorMask 0

        Pass {}
    }
}