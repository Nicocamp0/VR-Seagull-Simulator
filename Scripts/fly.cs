using UnityEngine;
using UnityEngine.XR;

public class fly : MonoBehaviour
{
    [Header("Nodes XR")]
    public XRNode mainGaucheNode = XRNode.LeftHand;
    public XRNode mainDroiteNode = XRNode.RightHand;

    [Header("Déplacement au sol")]
    public float vitesseSol = 6f;
    public float forceColleAuSol = 2f;

    [Header("Vitesse de vol")]
    public float vitesseVol = 5f;
    public float vitessemin = 2f;
    public float vitessemax = 35f;
    public float acceleration = 4f;
    public float deceleration = 3f;
    public float vitesseRotation = 30f;

    [Header("Seuils flap")]
    public float seuilMouvement = 0.06f;
    public float seuilInactivite = 0.12f;

    [Header("Gravité en vol")]
    public float graviteVol = 2.5f;
    public float delaiAvantChute = 0.6f;

    [Header("Décollage depuis le sol")]
    public float seuilDecollage = 0.8f;
    public float impulseDecollage = 5f;

    [Header("Piqué / boost")]
    public float multiplicateurVitessePique = 2f;
    public float accelerationPique = 8f;
    public float decelerationDepuisPique = 6f;
    public float vitesseDescentePique = 8f;
    public float lissageDescentePique = 6f;

    [Header("Détection piqué")]
    public float distanceMainTorse = 0.40f;
    public float tempsValidationPique = 0.15f;
    public float tempsMemoirePique = 0.20f;

    [Header("Position torse local (par rapport à la tête)")]
    public Vector3 torseLocalGauche = new Vector3(-0.25f, -0.55f, 0.15f);
    public Vector3 torseLocalDroite = new Vector3(0.25f, -0.55f, 0.15f);

    [Header("Détection verticale sous le casque")]
    public float hauteurMinSousCasque = 0.60f;
    public float toleranceHorizontaleBas = 0.55f;
    public float toleranceAvantBas = 0.45f;
    public float toleranceArriereBas = -0.20f;

    [Header("Debug")]
    public bool debugLogs = false;

    private InputDevice mainGauche;
    private InputDevice mainDroite;

    private Vector3 positionMainGauche;
    private Vector3 positionMainDroite;
    private Vector3 positionPrecedenteGauche;
    private Vector3 positionPrecedenteDroite;

    private Vector3 vitesseMainGauche;
    private Vector3 vitesseMainDroite;

    private CharacterController characterController;

    private float vitesseAvanceActuelle = 0f;
    private float tempsInactivite = 0f;
    private float vitesseChute = 0f;
    private float vitesseMontee = 0f;
    private float vitessePiqueVerticaleActuelle = 0f;

    private bool estEnVol = false;
    private bool estEnPique = false;
    private bool positionsInitialisees = false;

    private float timerSol = 0f;
    public float graceSol = 0.2f;

    private Transform cameraT;
    private Transform xrOriginT;
    private bool etaitAuSolAvant = true;
    private bool dejaDecolle = false;

    private float timerEntreePique = 0f;
    private float timerSortiePique = 0f;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
            characterController = gameObject.AddComponent<CharacterController>();

        characterController.radius = 0.2f;
        characterController.height = 0.5f;
        characterController.stepOffset = 0.3f;
        characterController.slopeLimit = 50f;

        cameraT = Camera.main.transform;
        xrOriginT = cameraT.parent;
    }

    void Update()
    {
        if (!mainGauche.isValid || !mainDroite.isValid)
        {
            mainGauche = InputDevices.GetDeviceAtXRNode(mainGaucheNode);
            mainDroite = InputDevices.GetDeviceAtXRNode(mainDroiteNode);
            return;
        }

        bool okG = mainGauche.TryGetFeatureValue(CommonUsages.devicePosition, out positionMainGauche);
        bool okD = mainDroite.TryGetFeatureValue(CommonUsages.devicePosition, out positionMainDroite);

        if (!okG || !okD)
            return;

        if (!positionsInitialisees)
        {
            positionPrecedenteGauche = positionMainGauche;
            positionPrecedenteDroite = positionMainDroite;
            positionsInitialisees = true;
            return;
        }

        vitesseMainGauche = (positionMainGauche - positionPrecedenteGauche) / Time.deltaTime;
        vitesseMainDroite = (positionMainDroite - positionPrecedenteDroite) / Time.deltaTime;

        bool groundedBrut = characterController.isGrounded;
        if (groundedBrut) timerSol = graceSol;
        else timerSol -= Time.deltaTime;

        bool auSol = timerSol > 0f;

        if (!dejaDecolle && etaitAuSolAvant && !auSol)
        {
            dejaDecolle = true;

            if (Score.instance != null)
            {
                Score.instance.RegisterHasFlown();
            }

            if (debugLogs)
                Debug.Log("Premier décollage détecté !");
        }

        etaitAuSolAvant = auSol;

        float flapGauche = Mathf.Max(0f, -vitesseMainGauche.y);
        float flapDroite = Mathf.Max(0f, -vitesseMainDroite.y);
        float flapMoyen = (flapGauche + flapDroite) * 0.5f;

        bool ailesActives =
            Mathf.Abs(vitesseMainGauche.y) > seuilInactivite ||
            Mathf.Abs(vitesseMainDroite.y) > seuilInactivite;

        Vector3 mainGaucheWorld = xrOriginT != null ? xrOriginT.TransformPoint(positionMainGauche) : positionMainGauche;
        Vector3 mainDroiteWorld = xrOriginT != null ? xrOriginT.TransformPoint(positionMainDroite) : positionMainDroite;

        Vector3 mainGaucheHeadLocal = cameraT.InverseTransformPoint(mainGaucheWorld);
        Vector3 mainDroiteHeadLocal = cameraT.InverseTransformPoint(mainDroiteWorld);

        float distanceGauche = Vector3.Distance(mainGaucheHeadLocal, torseLocalGauche);
        float distanceDroite = Vector3.Distance(mainDroiteHeadLocal, torseLocalDroite);
        bool mainGaucheContreCorps = distanceGauche < distanceMainTorse;
        bool mainDroiteContreCorps = distanceDroite < distanceMainTorse;
        bool mainGaucheAssezBasse = mainGaucheHeadLocal.y < -hauteurMinSousCasque;
        bool mainDroiteAssezBasse = mainDroiteHeadLocal.y < -hauteurMinSousCasque;
        bool mainGauchePasTropExterieure = Mathf.Abs(mainGaucheHeadLocal.x) < toleranceHorizontaleBas;
        bool mainDroitePasTropExterieure = Mathf.Abs(mainDroiteHeadLocal.x) < toleranceHorizontaleBas;
        bool mainGauchePasTropDevant = mainGaucheHeadLocal.z > toleranceArriereBas && mainGaucheHeadLocal.z < toleranceAvantBas;
        bool mainDroitePasTropDevant = mainDroiteHeadLocal.z > toleranceArriereBas && mainDroiteHeadLocal.z < toleranceAvantBas;
        bool mainGaucheConditionBas = mainGaucheAssezBasse && mainGauchePasTropExterieure && mainGauchePasTropDevant;
        bool mainDroiteConditionBas = mainDroiteAssezBasse && mainDroitePasTropExterieure && mainDroitePasTropDevant;

        bool conditionPique =
            (mainGaucheContreCorps || mainGaucheConditionBas) &&
            (mainDroiteContreCorps || mainDroiteConditionBas);

        if (conditionPique)
        {
            timerEntreePique += Time.deltaTime;
            timerSortiePique = 0f;

            if (timerEntreePique >= tempsValidationPique)
                estEnPique = true;
        }
        else
        {
            timerEntreePique = 0f;
            timerSortiePique += Time.deltaTime;

            if (timerSortiePique >= tempsMemoirePique)
                estEnPique = false;
        }

        if (debugLogs)
        {
            Debug.DrawLine(cameraT.TransformPoint(torseLocalGauche), mainGaucheWorld, mainGaucheContreCorps ? Color.green : Color.red);
            Debug.DrawLine(cameraT.TransformPoint(torseLocalDroite), mainDroiteWorld, mainDroiteContreCorps ? Color.green : Color.red);
        }

        if (estEnVol && auSol && vitesseMontee <= 0.1f)
        {
            estEnVol = false;
            estEnPique = false;
            vitesseChute = 0f;
            vitesseMontee = 0f;
            vitesseAvanceActuelle = 0f;
            vitessePiqueVerticaleActuelle = 0f;
            tempsInactivite = 0f;
            timerEntreePique = 0f;
            timerSortiePique = 0f;
        }

        if (!estEnVol && auSol && flapMoyen > seuilDecollage)
        {
            estEnVol = true;
            vitesseMontee = impulseDecollage;
            vitesseAvanceActuelle = vitessemin;
        }

        if (!estEnVol)
        {
            Vector2 joystickGauche;
            Vector2 joystickDroite;

            Vector3 mouvement = Vector3.zero;

            if (mainGauche.TryGetFeatureValue(CommonUsages.primary2DAxis, out joystickGauche)
                && joystickGauche.magnitude > 0.1f)
            {
                Vector3 camForward = Camera.main.transform.forward;
                Vector3 camRight = Camera.main.transform.right;

                camForward.y = 0f;
                camRight.y = 0f;
                camForward.Normalize();
                camRight.Normalize();

                mouvement = (camForward * joystickGauche.y + camRight * joystickGauche.x) * vitesseSol;
            }

            if (mainDroite.TryGetFeatureValue(CommonUsages.primary2DAxis, out joystickDroite))
            {
                float inputRotation = joystickDroite.x;

                if (Mathf.Abs(inputRotation) > 0.1f)
                {
                    transform.Rotate(Vector3.up, inputRotation * vitesseRotation * 2f * Time.deltaTime);
                }
            }

            Vector3 deplacementSol = mouvement * Time.deltaTime;
            deplacementSol += Vector3.down * forceColleAuSol * Time.deltaTime;

            characterController.Move(deplacementSol);
        }
        else
        {
            Vector3 directionVol = Camera.main.transform.forward;
            directionVol.y = 0f;

            if (directionVol.sqrMagnitude < 0.001f)
                directionVol = transform.forward;

            directionVol.Normalize();

            if (ailesActives)
            {
                float boost = flapMoyen * acceleration;
                vitesseAvanceActuelle = Mathf.Min(vitesseAvanceActuelle + boost * Time.deltaTime, vitessemax);

                tempsInactivite = Mathf.Max(0f, tempsInactivite - Time.deltaTime * 2f);
                vitesseChute = Mathf.Max(0f, vitesseChute - graviteVol * 2f * Time.deltaTime);

                if (vitesseMainGauche.y < -seuilMouvement && vitesseMainDroite.y < -seuilMouvement)
                {
                    float montee = (-vitesseMainGauche.y + -vitesseMainDroite.y) * 0.9f;
                    vitesseMontee = Mathf.Clamp(montee * vitesseVol, 0f, 2.5f);
                }
            }
            else
            {
                vitesseAvanceActuelle = Mathf.Max(vitessemin, vitesseAvanceActuelle - deceleration * Time.deltaTime);
                tempsInactivite += Time.deltaTime;

                if (tempsInactivite >= delaiAvantChute)
                    vitesseChute += graviteVol * Time.deltaTime;
            }

            if (estEnPique)
            {
                if (debugLogs)
                    Debug.Log("PIQUE DETECTE");

                float vitesseCible = vitessemax * multiplicateurVitessePique;

                vitesseAvanceActuelle = Mathf.MoveTowards(
                    vitesseAvanceActuelle,
                    vitesseCible,
                    accelerationPique * Time.deltaTime
                );

                vitessePiqueVerticaleActuelle = Mathf.MoveTowards(
                    vitessePiqueVerticaleActuelle,
                    vitesseDescentePique,
                    lissageDescentePique * Time.deltaTime
                );

                vitesseMontee = Mathf.MoveTowards(
                    vitesseMontee,
                    0f,
                    graviteVol * 3f * Time.deltaTime
                );
            }
            else
            {
                if (vitesseAvanceActuelle > vitessemax)
                {
                    vitesseAvanceActuelle = Mathf.MoveTowards(
                        vitesseAvanceActuelle,
                        vitessemax,
                        decelerationDepuisPique * Time.deltaTime
                    );
                }

                vitessePiqueVerticaleActuelle = Mathf.MoveTowards(
                    vitessePiqueVerticaleActuelle,
                    0f,
                    lissageDescentePique * Time.deltaTime
                );
            }

            Vector3 deplacement = directionVol * vitesseAvanceActuelle * Time.deltaTime;

            if (vitesseMontee > 0f)
            {
                deplacement += Vector3.up * vitesseMontee * Time.deltaTime;
                vitesseMontee = Mathf.Max(0f, vitesseMontee - graviteVol * Time.deltaTime);
            }

            deplacement -= Vector3.up * vitesseChute * Time.deltaTime;
            deplacement += Vector3.down * vitessePiqueVerticaleActuelle * Time.deltaTime;

            characterController.Move(deplacement);

            if (positionMainGauche.y > positionMainDroite.y + seuilMouvement)
                transform.Rotate(Vector3.up, vitesseRotation * Time.deltaTime);
            else if (positionMainDroite.y > positionMainGauche.y + seuilMouvement)
                transform.Rotate(Vector3.up, -vitesseRotation * Time.deltaTime);
        }

        positionPrecedenteGauche = positionMainGauche;
        positionPrecedenteDroite = positionMainDroite;
    }
}