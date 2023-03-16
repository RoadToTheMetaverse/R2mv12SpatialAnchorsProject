using UnityEngine;

namespace R2mv12Assets.Scripts
{
    public class TapToEmit : MonoBehaviour
    {

        [SerializeField] private ParticleSystem _particles;
    
        // Start is called before the first frame update
        void Start()
        {
            if(_particles == null)
                Destroy(this);
        }

        // Update is called once per frame
        void Update()
        {
            if ((Input.touchCount > 0) && (Input.GetTouch(0).phase == TouchPhase.Began))
            {
                Ray raycast = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
                RaycastHit raycastHit;
                if (Physics.Raycast(raycast, out raycastHit))
                {
                    if (raycastHit.collider.name == gameObject.name)
                    {
                        _particles.Emit(100);
                    }
                }
            }
        }
    }
}
