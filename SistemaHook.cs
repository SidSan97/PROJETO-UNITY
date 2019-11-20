﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

//codigo incompleto
// codigo feito com base no tutorial do site https://www.raywenderlich.com/348-make-a-2d-grappling-hook-game-in-unity-part-1 feito por Sean Duffy

public class SistemaHook : MonoBehaviour
{
    public GameObject RopeHingeAnchor; //traz o gameObject
    public DistanceJoint2D ropeJoint; //cria a conexao entre dois pontos
    public Transform crosshair;//movimentacao da mira da personagem
    public SpriteRenderer crosshairSprite; //desenha o sprite da mira
    public PlayerMove playerMovement;
    private bool ropeAttached; //confere se a corda esta ou nao conectada a algo
    private Vector2 playerPosition; //controla a posicao do player (?)
    private Rigidbody2D ropeHingeAnchorRb; //cria a fisica basica do gancho
    private SpriteRenderer ropeHingeAnchorSprite;// desenha o sprite do gancho

    public LineRenderer ropeRenderer; //responsavel por desenhar uma linha
    public LayerMask ropeLayerMask; // configura qual layer o raycast do hook se movera
    private float ropeMaxCastDistance = 20f; //define o range maximo do hook
    private List<Vector2> ropePositions = new List<Vector2>(); //esta lista vai trackear os pontos de dobra na corda caso entre em colisao com algum objeto

    void Awake()// inicializa as funcoes
    {
        ropeJoint.enabled = false; //comeca o DJ2D desligado, para ser ativado ao comando do player (?)
        playerPosition = transform.position; //da "track" na posicao do player, e seta a posicao inicial do player
        ropeHingeAnchorRb = RopeHingeAnchor.GetComponent<Rigidbody2D>(); //atrela o RigidBody ao objeto do gancho
        ropeHingeAnchorSprite = RopeHingeAnchor.GetComponent<SpriteRenderer>(); //atrela um spriteRenderer ao gancho

    }

    void Update()
    {
        var worldMousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f)); //traz a posicao do mouse de uma realidade 2d para uma 3d com o vector3 para realizar a operacao com o transform.position
        var facingDirection = worldMousePosition - transform.position; // subtrai a posicao do player da posicao do mouse no mundo e armazena no vetor para descobrir qual a direcao que o cursor esta apontando
        var aimAngle = Mathf.Atan2(facingDirection.y, facingDirection.x); //retorna o angulo entre o eixo x do jogo e a posicao que o mouse esta

        if (aimAngle < 0f)
        {
            aimAngle = Mathf.PI * 2 + aimAngle;// multiplica Pi por 2 e soma ao aimAngle para ... (?)
        }

        var aimDirection = Quaternion.Euler(0, 0, aimAngle * Mathf.Rad2Deg) * Vector2.right;// retorna o angulo criado na rotacao do eixo z em graus
        playerPosition = transform.position;

        if (!ropeAttached)
        {
            SetCrosshairPosition(aimAngle);
        }
        else
        {
            crosshairSprite.enabled = false;
        }

        HandleInput(aimDirection);

    }
    private void SetCrosshairPosition(float aimAngle) // configura uma funcao para decidir a posicao da mira
    {
        if (!crosshairSprite.enabled)// se estiver disabled torna o sprite enabled
        {
            crosshairSprite.enabled = true;
        }

        var x = transform.position.x + 1f * Mathf.Cos(aimAngle); // configura a posicao x da mira baseando-se na posicao do player e no angulo formado pela posicao do mouse
        var y = transform.position.y + 1f * Mathf.Sin(aimAngle);// configura a posicao y da mira

        var crossHairPosition = new Vector3(x, y, 0); //cria um vector3 para guardar a posicao da mira
        crosshair.transform.position = crossHairPosition;


    }

    private void HandleInput(Vector2 aimDirection)// funcao que desenha a corda e o raycast da fisica
    {
        if (Input.GetMouseButton(0)) //ativa quando o botao esquerdo do mouse e pressionado
        {
            if (ropeAttached) return;

            ropeRenderer.enabled = true;

            var hit = Physics2D.Raycast(playerPosition/*origem do raycast*/, aimDirection/*direcao do raycast*/, ropeMaxCastDistance/*tamanho maximo do raycast*/, ropeLayerMask/*layer do raycast*/);

            if (hit.collider != null) // se existir uma colisao com um raycast valido
            {
                ropeAttached = true;// confirma que o hook esta agarrado em algo
                if (!ropePositions.Contains(hit.point))// ocorrera caso o hook n encosta em algum lugar
                {
                    transform.GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, 2f), ForceMode2D.Impulse);// uma forca e adicionada a personagem
                    ropePositions.Add(hit.point);//adiciona um ponto de contato entre o terreno e a corda
                    ropeJoint.distance = Vector2.Distance(playerPosition, hit.point);// retorna a distancia entre o player e o ponto de contato do hook
                    ropeJoint.enabled = true;
                    ropeHingeAnchorSprite.enabled = true;// ativa o sprite do gancho
                }
            }

            else// caso o raycast n colida com nada
            {
                ropeRenderer.enabled = false;
                ropeAttached = false;
                ropeJoint.enabled = false;
            }
            if (Input.GetMouseButton(1))// se o botao direito do mouse for clicado, a corda e resetada
            {
                ResetRope();
            }
        }
    }
    private void ResetRope()
    {
        ropeJoint.enabled = false;
        ropeAttached = false;
        playerMovement.isSwinging = false;
        ropeRenderer.positionCount = 2;
        ropeRenderer.SetPosition(0, transform.position);
        ropeRenderer.SetPosition(1, transform.position);
        ropePositions.Clear();
        ropeHingeAnchorSprite.enabled = false;
    }
}
