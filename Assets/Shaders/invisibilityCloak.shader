// TODO: Make non-invisible bird sprites show up in front of these
 // non-shader tricks: make bird sprite always render in front, but when it's clicked I swap it out for a bird sprite that has a cube taken out of it
 // non-shader tricks: make bird further forward and scaled down so it looks the same size but geometry renders it in front
// TODO: draw outlines of cubes
// TODO: cube blurs baackground
Shader "Shaders/InvisibilityCloak" {
   
        SubShader
        {
            Tags { "Queue"="Transparent"}
       
            Pass
            {
                ZWrite On
                Blend Zero One
                BlendOp Add
            }
       
         }//end subshader
}//end shader