Shader "Custom/D3" {  
 Properties {  
        _MainTex ("Base (RGB)", 2D) = "white" {}  
        _Mask ("Base (RGB)", 2D) = "white" {}  
    }  
    SubShader {  
        Tags{"Queue"="Transparent"}  
        Pass{         
  
            blend SrcAlpha OneMinusDstAlpha  
          
            SetTexture [_MainTex]{  
                combine  texture   
            }  
              
            SetTexture [_Mask]{  
                combine  Previous , texture  
            }  
        }  
          
    }   
}  