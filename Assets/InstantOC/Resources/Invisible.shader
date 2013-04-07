Shader "Custom/Invisible" {
    Subshader
    {
       UsePass "VertexLit/SHADOWCOLLECTOR"    
       UsePass "VertexLit/SHADOWCASTER"
    }
 
    Fallback off
}