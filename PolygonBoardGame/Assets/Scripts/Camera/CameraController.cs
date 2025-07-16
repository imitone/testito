using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    public Camera mainCamera;
    public float followSpeed = 5f;
    public float rotationSpeed = 2f;
    public float zoomSpeed = 3f;
    
    [Header("Board View")]
    public Vector3 boardViewPosition = new Vector3(0, 15, -10);
    public Vector3 boardViewRotation = new Vector3(45, 0, 0);
    public float boardViewSize = 20f;
    
    [Header("Player Follow")]
    public Vector3 playerFollowOffset = new Vector3(0, 8, -8);
    public float playerFollowHeight = 5f;
    public float maxFollowDistance = 15f;
    
    [Header("Mini-Game View")]
    public Vector3 miniGamePosition = new Vector3(0, 10, -15);
    public Vector3 miniGameRotation = new Vector3(30, 0, 0);
    public float miniGameSize = 15f;
    
    [Header("Smooth Transitions")]
    public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float transitionDuration = 2f;
    
    private Transform currentTarget;
    private Vector3 targetPosition;
    private Vector3 targetRotation;
    private float targetSize;
    private bool isTransitioning = false;
    private CameraState currentState = CameraState.Board;
    
    public enum CameraState
    {
        Board,
        PlayerFollow,
        MiniGame,
        Transition
    }
    
    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        if (mainCamera == null)
        {
            mainCamera = GetComponent<Camera>();
        }
        
        SetupBoardView();
    }
    
    void Update()
    {
        HandleInput();
        UpdateCameraPosition();
    }
    
    void HandleInput()
    {
        // Manual camera controls (for testing/debugging)
        if (Input.GetKeyDown(KeyCode.B))
        {
            SetBoardView();
        }
        
        if (Input.GetKeyDown(KeyCode.M))
        {
            SetMiniGameView();
        }
        
        // Zoom controls
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0 && !isTransitioning)
        {
            ZoomCamera(scrollInput);
        }
    }
    
    void UpdateCameraPosition()
    {
        if (isTransitioning) return;
        
        switch (currentState)
        {
            case CameraState.Board:
                UpdateBoardView();
                break;
            case CameraState.PlayerFollow:
                UpdatePlayerFollow();
                break;
            case CameraState.MiniGame:
                UpdateMiniGameView();
                break;
        }
    }
    
    void UpdateBoardView()
    {
        // Keep camera at board view position
        Vector3 desiredPosition = boardViewPosition;
        Vector3 desiredRotation = boardViewRotation;
        
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * followSpeed);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(desiredRotation), Time.deltaTime * rotationSpeed);
        
        if (mainCamera.orthographic)
        {
            mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, boardViewSize, Time.deltaTime * zoomSpeed);
        }
    }
    
    void UpdatePlayerFollow()
    {
        if (currentTarget == null) return;
        
        Vector3 desiredPosition = currentTarget.position + playerFollowOffset;
        desiredPosition.y = playerFollowHeight;
        
        // Clamp to maximum follow distance
        Vector3 boardCenter = boardViewPosition;
        boardCenter.y = 0;
        float distanceFromCenter = Vector3.Distance(new Vector3(desiredPosition.x, 0, desiredPosition.z), boardCenter);
        
        if (distanceFromCenter > maxFollowDistance)
        {
            Vector3 directionFromCenter = (desiredPosition - boardCenter).normalized;
            desiredPosition = boardCenter + directionFromCenter * maxFollowDistance;
            desiredPosition.y = playerFollowHeight;
        }
        
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * followSpeed);
        
        // Look at target
        Vector3 lookDirection = (currentTarget.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }
    
    void UpdateMiniGameView()
    {
        Vector3 desiredPosition = miniGamePosition;
        Vector3 desiredRotation = miniGameRotation;
        
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * followSpeed);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(desiredRotation), Time.deltaTime * rotationSpeed);
        
        if (mainCamera.orthographic)
        {
            mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, miniGameSize, Time.deltaTime * zoomSpeed);
        }
    }
    
    public void SetBoardView()
    {
        StartCoroutine(TransitionToState(CameraState.Board));
    }
    
    public void SetMiniGameView()
    {
        StartCoroutine(TransitionToState(CameraState.MiniGame));
    }
    
    public void FocusOnPlayer(Player player)
    {
        if (player != null)
        {
            currentTarget = player.transform;
            StartCoroutine(TransitionToState(CameraState.PlayerFollow));
        }
    }
    
    public void ReturnToBoard()
    {
        currentTarget = null;
        SetBoardView();
    }
    
    IEnumerator TransitionToState(CameraState newState)
    {
        if (isTransitioning) yield break;
        
        isTransitioning = true;
        currentState = CameraState.Transition;
        
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        float startSize = mainCamera.orthographic ? mainCamera.orthographicSize : mainCamera.fieldOfView;
        
        Vector3 endPosition;
        Quaternion endRotation;
        float endSize;
        
        // Determine target values based on state
        switch (newState)
        {
            case CameraState.Board:
                endPosition = boardViewPosition;
                endRotation = Quaternion.Euler(boardViewRotation);
                endSize = boardViewSize;
                break;
            case CameraState.PlayerFollow:
                if (currentTarget != null)
                {
                    endPosition = currentTarget.position + playerFollowOffset;
                    endPosition.y = playerFollowHeight;
                    endRotation = Quaternion.LookRotation((currentTarget.position - endPosition).normalized);
                }
                else
                {
                    endPosition = startPosition;
                    endRotation = startRotation;
                }
                endSize = boardViewSize * 0.7f;
                break;
            case CameraState.MiniGame:
                endPosition = miniGamePosition;
                endRotation = Quaternion.Euler(miniGameRotation);
                endSize = miniGameSize;
                break;
            default:
                endPosition = startPosition;
                endRotation = startRotation;
                endSize = startSize;
                break;
        }
        
        // Perform smooth transition
        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;
            float curveValue = transitionCurve.Evaluate(t);
            
            transform.position = Vector3.Lerp(startPosition, endPosition, curveValue);
            transform.rotation = Quaternion.Lerp(startRotation, endRotation, curveValue);
            
            if (mainCamera.orthographic)
            {
                mainCamera.orthographicSize = Mathf.Lerp(startSize, endSize, curveValue);
            }
            else
            {
                mainCamera.fieldOfView = Mathf.Lerp(startSize, endSize, curveValue);
            }
            
            yield return null;
        }
        
        // Ensure final values are set
        transform.position = endPosition;
        transform.rotation = endRotation;
        
        if (mainCamera.orthographic)
        {
            mainCamera.orthographicSize = endSize;
        }
        else
        {
            mainCamera.fieldOfView = endSize;
        }
        
        currentState = newState;
        isTransitioning = false;
    }
    
    void SetupBoardView()
    {
        transform.position = boardViewPosition;
        transform.rotation = Quaternion.Euler(boardViewRotation);
        
        if (mainCamera.orthographic)
        {
            mainCamera.orthographicSize = boardViewSize;
        }
        else
        {
            mainCamera.fieldOfView = boardViewSize;
        }
        
        currentState = CameraState.Board;
    }
    
    public void ZoomCamera(float zoomAmount)
    {
        if (mainCamera.orthographic)
        {
            float newSize = mainCamera.orthographicSize - zoomAmount * zoomSpeed;
            mainCamera.orthographicSize = Mathf.Clamp(newSize, 5f, 50f);
        }
        else
        {
            float newFOV = mainCamera.fieldOfView - zoomAmount * zoomSpeed * 10f;
            mainCamera.fieldOfView = Mathf.Clamp(newFOV, 20f, 80f);
        }
    }
    
    public void SetCameraMode(bool orthographic)
    {
        mainCamera.orthographic = orthographic;
        
        if (orthographic)
        {
            mainCamera.orthographicSize = boardViewSize;
        }
        else
        {
            mainCamera.fieldOfView = 60f;
        }
    }
    
    public void ShakeCamera(float intensity, float duration)
    {
        StartCoroutine(CameraShake(intensity, duration));
    }
    
    IEnumerator CameraShake(float intensity, float duration)
    {
        Vector3 originalPosition = transform.position;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;
            float z = Random.Range(-1f, 1f) * intensity;
            
            transform.position = originalPosition + new Vector3(x, y, z);
            
            yield return null;
        }
        
        transform.position = originalPosition;
    }
    
    public void FocusOnPosition(Vector3 position, float duration = 2f)
    {
        StartCoroutine(FocusOnPositionCoroutine(position, duration));
    }
    
    IEnumerator FocusOnPositionCoroutine(Vector3 targetPos, float duration)
    {
        if (isTransitioning) yield break;
        
        isTransitioning = true;
        
        Vector3 startPosition = transform.position;
        Vector3 endPosition = targetPos + playerFollowOffset;
        endPosition.y = playerFollowHeight;
        
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float curveValue = transitionCurve.Evaluate(t);
            
            transform.position = Vector3.Lerp(startPosition, endPosition, curveValue);
            
            yield return null;
        }
        
        transform.position = endPosition;
        isTransitioning = false;
    }
    
    public CameraState GetCurrentState()
    {
        return currentState;
    }
    
    public bool IsTransitioning()
    {
        return isTransitioning;
    }
    
    // Utility methods for game events
    public void OnPlayerTurnStart(Player player)
    {
        if (currentState == CameraState.Board)
        {
            // Briefly focus on player, then return to board
            StartCoroutine(BriefPlayerFocus(player));
        }
    }
    
    IEnumerator BriefPlayerFocus(Player player)
    {
        Vector3 originalPosition = transform.position;
        Vector3 playerPosition = player.transform.position + playerFollowOffset;
        playerPosition.y = playerFollowHeight;
        
        // Move to player
        yield return StartCoroutine(FocusOnPositionCoroutine(player.transform.position, 1f));
        
        // Wait briefly
        yield return new WaitForSeconds(0.5f);
        
        // Return to board view
        yield return StartCoroutine(TransitionToState(CameraState.Board));
    }
    
    public void OnMiniGameStart()
    {
        SetMiniGameView();
    }
    
    public void OnMiniGameEnd()
    {
        ReturnToBoard();
    }
    
    public void OnPropertyTransaction(BoardSpace space)
    {
        if (space != null)
        {
            FocusOnPosition(space.transform.position, 1f);
        }
    }
}