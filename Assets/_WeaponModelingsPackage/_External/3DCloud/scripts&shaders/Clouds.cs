using UnityEngine;
using System.Collections;


public class Clouds : MonoBehaviour {


	[Header("Cloud Density:")]
	[Range(0.0f, 1.0f)]
	public float density = .3f;//density variable - change from editor

	[Header("Quality(Number of particles):")]
	[Range(1, 10)]
	public int quality = 10;// You need to decrease this value for mobile devices
	[Header("Resolution:")]
	public int height =128;//resolution 128x128 is fine for standalone and web applications, should be reduced for mobile
	public int width =128;
	private int depth =2;
	[Header("Clouds area scale:")]
	public float clouds_scale=20f;// main scaling parameter

	[Header("Clouds size scale:")]
	public float clouds_size=20f;// main scaling parameter

	[Header("Clouds average lifetime:")]
	public float clouds_lifetime=6f;// main scaling parameter

	[Header("Clouds speed:")]
	[Range(0.0f, 10.0f)]
	public float speed_k_value=1f;

	[Header("Clouds darkness:")]
	[Range(0.0f, 1.0f)]
	public float darkness_max = .45f; // maximum cloud darkness

	[Header("Show GUI:")]

	public bool show_gui = true; // to show or not to show...gui =)
	

	[HideInInspector]
	public float speed_k;

	[HideInInspector]
	public float transparency_extra = 0f;


	[HideInInspector]
	public float treshold;
	[HideInInspector]
	public float[,] noise ;
	[HideInInspector]
	public float particle_creation_k_value ;
	[HideInInspector]
	public float distance_covered=0f;
	[HideInInspector]
	public float lifetime_base = 6f;//basic lifetime


	private float xOrg;
	private float yOrg;
	private float darkness_extra_old = 0f;
	private float darkness_extra = 0.2f; // average darkness - is being changed by slider
	private float transparency_extra_old = 0f;
	private int randomSeed_x;
	private int randomSeed_y;
	private float scale = 10F;//noise scale
	private float treshold_old;
	private Vector3[] Rand;
	private Color[] Colors;
	private float[] cls_size;
	private float delta_t=0f;
	private float old_t=0f;
	private float cloud_size_extra=1f;
	private float cloud_size_extra_old=1f;
	private Color[] pix;
	private bool first_time=true;

	private float day_speed=1f;// this variable is used to change clouds speed and lifetime together
	private float day_speed_old;
	void Start() {

		speed_k =speed_k_value*clouds_scale;
		Rand = new Vector3[width*height*depth];// create arrays to store random positions of the particles,there creation colors and sizes
		Colors = new Color[width*height*depth];//
		cls_size = new float[width*height*depth];//
		treshold = 1f - density;// treshold value is used to create a perlin noise based clouds map(0...1)
		treshold_old= treshold;
		particle_creation_k_value = density*10f;// just a koefficient
		this.GetComponent<ParticleSystem>().maxParticles = 1000000;
		this.GetComponent<ParticleSystem>().startRotation = Random.Range(0f,360f);// I don't think that it's really neccessary, but, well...
		noise = new float[width, height];
		randomSeed_x = Random.Range(1,1000);// random seed to randomize perlin noise
		randomSeed_y = Random.Range(1,1000);
		xOrg+=randomSeed_x;
		yOrg+=randomSeed_y;
		CalcSpeed();
		CalcNoise();//generating noise
		Createps();
		InvokeRepeating("UpdateOldps",.05f,.05f);//  moving already created particles

	}

	void OnGUI(){//--basic gui controls
	if (show_gui)
		{
			GUIStyle blStyle = new GUIStyle();
			blStyle.normal.textColor = Color.black;
			GUI.Label(new Rect(10,25,200,20),"Clouds Density:",blStyle);
			density = Mathf.CeilToInt(GUI.HorizontalSlider(new Rect(10,45,130,20),Mathf.CeilToInt(density*10f),1,10))/10f;// Some math was added just to quantize the density
			//print(density);
			treshold = 1f - density;
			particle_creation_k_value = density*10f;
			if (treshold!=treshold_old){
				treshold_old=treshold;
				Createps();
			}
			GUI.Label(new Rect(150,25,200,20),"Clouds Darkness:",blStyle);
			darkness_extra =GUI.HorizontalSlider(new Rect(150,45,130,20),darkness_extra,0f,1f);
			if (darkness_extra!=darkness_extra_old){
				darkness_extra_old = darkness_extra;
				ChangeColor();
			}
			GUI.Label(new Rect(300,25,200,20),"Clouds Transparency:",blStyle);
			transparency_extra =GUI.HorizontalSlider(new Rect(300,45,130,20),transparency_extra,-.5f,.5f);
			if (transparency_extra!=transparency_extra_old){
				transparency_extra_old = transparency_extra;
				ChangeColor();
				this.transform.Find("Shadow").gameObject.SendMessage("ChangeShadow_transparency");
			}
			GUI.Label(new Rect(450,25,200,20),"Clouds Size:",blStyle);
			cloud_size_extra =GUI.HorizontalSlider(new Rect(450,45,130,20),cloud_size_extra,0.5f,2f);
			if (cloud_size_extra!=cloud_size_extra_old){
				cloud_size_extra_old = cloud_size_extra;
				ChangeSize();
			}
			GUI.Label(new Rect(600,25,200,20),"Clouds Speed and lifetime:",blStyle);
			day_speed =Mathf.CeilToInt(GUI.HorizontalSlider(new Rect(600,45,130,20),Mathf.CeilToInt(day_speed),1f,10f));
			if (day_speed!=day_speed_old){
				day_speed_old = day_speed;
				CalcSpeed();
				Createps();
			}
		}
	}//---

	void CalcSpeed(){//calculating interconnected lifetime and speed
		if (show_gui){ //if we control it using gui
			clouds_lifetime= lifetime_base/day_speed;

			speed_k_value = 0.5f*day_speed*day_speed;
			speed_k =speed_k_value*clouds_scale;
		} else{//if we use values from editor
			speed_k =speed_k_value*clouds_scale;
		}
	}

//simple 2d perlin noise
	void CalcNoise() {
		float y = 0.0F;
		while (y < height) {
			float x = 0.0F;
			while (x < width) {

				
				float xCoord = (xOrg + x) / width;
				float yCoord = (yOrg + y) / height;

				float sample = Mathf.PerlinNoise(xCoord* scale, yCoord* scale)+Mathf.PerlinNoise(xCoord*2* scale, yCoord*2* scale)/2+Mathf.PerlinNoise(xCoord*4* scale, yCoord*4* scale)/4+Mathf.PerlinNoise(xCoord*8* scale, yCoord*8* scale)/8-.3f;

				noise[(int)x,(int)y] = sample;
				x++;
			}
			y++;
		}

	}

//this function is responsible for controlling particle colors
	void ChangeColor(){
		ParticleSystem.Particle[] particles=new ParticleSystem.Particle[this.GetComponent<ParticleSystem>().particleCount];

		int count = this.GetComponent<ParticleSystem>().GetParticles(particles);
		for (int i=0;i<count;i++){

				particles[i].color =  new Color(Colors[i].r - darkness_extra,Colors[i].g - darkness_extra,Colors[i].b - darkness_extra,Colors[i].a+transparency_extra) ;
		}
		this.GetComponent<ParticleSystem>().SetParticles(particles,count);
	}

	void ChangeSize(){
		ParticleSystem.Particle[] particles=new ParticleSystem.Particle[this.GetComponent<ParticleSystem>().particleCount];
		
		int count = this.GetComponent<ParticleSystem>().GetParticles(particles);
		for (int i=0;i<count;i++){
				particles[i].size =  cls_size[i]*cloud_size_extra ;

		}
		this.GetComponent<ParticleSystem>().SetParticles(particles,count);
	}

//This function is creating particles, is called when the particles should be created/recreated
	void Createps(){
		ParticleSystem.Particle[] particles=new ParticleSystem.Particle[width*height*depth];

		int count = this.GetComponent<ParticleSystem>().GetParticles(particles);
		int p_i=0;
		for (int i=0;i < (width);i++){
			for (int j=0;j<(height);j++)
			{

					//Random.Range(-20+quality+Mathf.Ceil(particle_creation_k_value),10)
				if (noise[i,j]>=treshold && Random.Range(0,10*particle_creation_k_value/Mathf.Sqrt(quality))<10){//particle is created if noise level at this point is higher, than treshold,
					//second statement just ensures that the number of particles depends on a quality parameter and stays the same independently from the density.

					for (int d=0;d<depth;d++){



						//--- Here are math equations defining main particle parameteres
						float darkness =Mathf.Lerp(0f,darkness_max,(noise[i,j]-treshold)/(1f-treshold));
						float transparency = Mathf.Lerp(0.6f,Random.Range(0.6f,.1f),(noise[i,j]-treshold)/(1f-treshold));

						if (noise[i,j]>.6f){
							particles[p_i].size = Random.Range(3f,5f)*noise[i,j]*noise[i,j]*clouds_size*2f*Mathf.Pow(particle_creation_k_value,1/2.5f);
						} else {
							particles[p_i].size =Random.Range(3f,5f)*.36f*clouds_size*2f*Mathf.Pow(particle_creation_k_value,1/2.5f);
						}


						cls_size[p_i] = particles[p_i].size;
						Colors[p_i] = new Color(1f-darkness,1f-darkness,1f-darkness,transparency);
						Rand[p_i] =Random.insideUnitSphere/2f;

						particles[p_i].color = Colors[p_i] ;

						float new_pos_z =(j-height/2f)*clouds_scale + distance_covered;
						if (new_pos_z>height*clouds_scale)
							new_pos_z -= height*clouds_scale;
						particles[p_i].position = this.transform.position + new Vector3((i-width/2f)*clouds_scale,(d-depth/2f+Random.Range(0f,1f))*((noise[i,j]-treshold)/(1f-treshold))*clouds_scale*2f,new_pos_z)+Rand[p_i];

						float life = (Random.Range(2f*clouds_lifetime/3f,4f*clouds_lifetime/3f));

						particles[p_i].startLifetime = life;
						particles[p_i].remainingLifetime = Mathf.Lerp(0.1f,particles[p_i].startLifetime,Random.Range(0f,1f));
						particles[p_i].randomSeed=(uint)Random.Range(0,6);
					
						particles[p_i].rotation = Random.Range(-180f,180f);
						//---

						p_i++;
					}

				}

			}

	}
		this.transform.Find("Shadow").gameObject.SendMessage("SetNoise");// we provide info to shadows creating script
		this.transform.Find("Shadow").gameObject.SendMessage("SetSpeed");// we provide info to shadows creating script

		this.GetComponent<ParticleSystem>().SetParticles(particles,p_i);//applying all particle changes
		ChangeColor();
		ChangeSize();

	}

	void UpdateOldps(){
		ParticleSystem.Particle[] particles=new ParticleSystem.Particle[this.GetComponent<ParticleSystem>().particleCount];
		delta_t = Time.time-old_t;
		old_t = Time.time;//checking how much time passed since last movement(to be sure)
		int count = this.GetComponent<ParticleSystem>().GetParticles(particles);
		distance_covered+=speed_k*delta_t;

		if (distance_covered>=height*clouds_scale)
			distance_covered-=height*clouds_scale;
		this.transform.Find("Shadow").gameObject.SendMessage("SetDistance");
		for (int i=0;i<count;i++){
			particles[i].position+=new Vector3(0f,0f,speed_k*delta_t);
			//if particle is dying we check if it've crossed the border already, if yes - "recreating" it on the other side, else at the same position
			if (particles[i].remainingLifetime<particles[i].startLifetime*.05f){
				particles[i].remainingLifetime=particles[i].startLifetime;
				if(particles[i].position.z>this.transform.position.z+height*clouds_scale/2f){
					float z_delta = particles[i].position.z - (this.transform.position.z+height*clouds_scale/2f);
					particles[i].position = new Vector3(particles[i].position.x,particles[i].position.y,this.transform.position.z-height*clouds_scale/2f+z_delta);
					particles[i].remainingLifetime=particles[i].startLifetime;
				}
			}

		

		
		}
		this.GetComponent<ParticleSystem>().SetParticles(particles,count);
	}



}
