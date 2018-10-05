using UnityEngine;
using System.Collections;

public class BeamShot : MonoBehaviour
{

    private ParticleSystem beamParticle;
    private GameObject beam;
    private int buttonPushNum;
    private bool isBeam;

    // Use this for initialization
    void Awake()
    {
        beamParticle = GetComponent<ParticleSystem>();
        beamParticle.Stop();
        beam = GameObject.Find("Capsule");
        beam.SetActive(false);
        buttonPushNum = 0;
        isBeam = true;
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            if(isBeam)Charge();
            buttonPushNum++;
        }
        Debug.Log(CarData.carSpeed);
    }

    private void Charge()
    {
        isBeam = false;
        beamParticle.Play();

        StartCoroutine(Shot());
        
    }
    
    private IEnumerator Shot()
    {

        yield return new WaitForSeconds(2.0f);
        CarData.carSpeed = 2.0f * (float)buttonPushNum;
        beam.SetActive(true);
       
        yield return new WaitForSeconds(2.0f);
        beam.SetActive(false);
        beamParticle.Stop();

    }
}


