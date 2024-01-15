using UnityEngine;
using System.Collections;

public class Shadows : MonoBehaviour {
	private float[,] noise ;
	private int height;
	private int width ;
	private Texture2D noiseTex;
	private Color[] pix;
	private float treshold;
	private float speed_k;
	private int smooth_rad = 4;
	private int resize_k = 2;
	private float shadow_density;
	private float transparency_value=0.5f;
	private float clouds_scale;
	private float shadow_transparency;
	private float distance;
	// Use this for initialization
	void Awake () {

		//getting basic parameters from clouds script
		height = this.transform.parent.transform.GetComponent<Clouds>().height;
		width = this.transform.parent.transform.GetComponent<Clouds>().width;
		clouds_scale = this.transform.parent.transform.GetComponent<Clouds>().clouds_scale;
		noise = new float[width,height];

		noiseTex = new Texture2D(width*resize_k, height*resize_k);
		pix = new Color[noiseTex.width * noiseTex.height];
		this.transform.GetComponent<Projector>().material.SetTexture("_ShadowTex",noiseTex);
		this.transform.GetComponent<Projector>().orthographicSize = 1250 * height * clouds_scale/(128f*20f);
		this.transform.GetComponent<Projector>().material.SetFloat("_Transparency",.5f);


	}
	//shadow transparency control
	public void ChangeShadow_transparency(){
		shadow_transparency = this.transform.parent.transform.GetComponent<Clouds>().transparency_extra;
		this.transform.GetComponent<Projector>().material.SetFloat("_Transparency",transparency_value+shadow_transparency);
	}
	//applying noise to shadow script
	public void SetNoise(){
		noise = this.transform.parent.transform.GetComponent<Clouds>().noise;
		treshold = this.transform.parent.transform.GetComponent<Clouds>().treshold;

		shadow_density = this.transform.parent.transform.GetComponent<Clouds>().density;

		ApplyTexture();
	}
	public void SetSpeed(){

		speed_k = this.transform.parent.transform.GetComponent<Clouds>().speed_k;
	

	}
	public void SetDistance(){
		distance = this.transform.parent.transform.GetComponent<Clouds>().distance_covered;
		this.transform.GetComponent<Projector>().material.SetFloat("_Distance",distance/(height*clouds_scale));
	}

	//applying texture with smoothing(noise size is small, we increase it up to 1024x1024)
	void ApplyTexture(){
		for (int j=0;j<(height*resize_k);j++){
			for (int i=0;i<(width*resize_k);i++){
				pix[(int)(j * width*resize_k + i)] =new Color(1f,1f,1f);
			}
		}
		if(shadow_density<.75f){//if clouds are not dense we are leaving some soft borders for neat tiling
			for (int j=0+smooth_rad;j<(height*resize_k-smooth_rad);j+=smooth_rad/2){
				for (int i=0+smooth_rad;i<(width*resize_k-smooth_rad);i+=smooth_rad/2){
				
					float col = Mathf.Lerp(0.5f,1f,(1f-noise[i/resize_k,j/resize_k])*Mathf.Lerp(.5f,4.5f,1f-shadow_density)); //noise[i,j]*Mathf.Lerp(1f,0f,Mathf.Pow((treshold-noise[i,j]),3f)); 

					Circle(noiseTex,i,j,smooth_rad,col);


					
				}
			}
		} else{//else we fill the whole are
			print ("__");
			for (int j=0;j<(height*resize_k);j+=smooth_rad/2){
				for (int i=0;i<(width*resize_k);i+=smooth_rad/2){
					
					float col = Mathf.Lerp(0.5f,1f,(1f-noise[i/resize_k,j/resize_k])*Mathf.Lerp(.5f,4.5f,1f-shadow_density)); //noise[i,j]*Mathf.Lerp(1f,0f,Mathf.Pow((treshold-noise[i,j]),3f)); 
					
					Circle(noiseTex,i,j,smooth_rad,col);
					
					
					
				}
			}
		}



		noiseTex.SetPixels(pix);
		noiseTex.Apply();

	}



	//Smoothing algorithm using circles
	public void Circle(Texture2D tex, int cx, int cy, int r, float col)
	{
		float cur_col=0f;;

		for(int j = cy-r;j<cy+r;j++){
			if (j<0 || j>= height*resize_k)
				continue;
			for(int i = cx-r;i<cx+r;i++){
				if (i<0 || i>= width*resize_k)
					continue;
				int sqr_d = (cx-i)*(cx-i)+(cy-j)*(cy-j);
				if(sqr_d<r*r){
					//if(pix[(int)(j * width*resize_k + i)].r!=0f){
						cur_col = pix[(int)(j * width*resize_k + i)].r*Mathf.Lerp(col,1f,Mathf.Sqrt(sqr_d)/r);
					//}
					 pix[(int)(j * width*resize_k + i)] = new Color(cur_col,cur_col,cur_col);
				}
			}
		}

	}

}
