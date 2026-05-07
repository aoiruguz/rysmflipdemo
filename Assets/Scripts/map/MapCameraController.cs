using UnityEngine;

public class MapCameraController : MonoBehaviour
{
    [Header("ê������ (0Ϊ���·�)")]
    public Transform[] anchors;

    [Header("�ƶ�����")]
    public float smoothSpeed = 8f;      // �ƶ��ٶȣ�ֵԽ��Խ��
    public float scrollCooldown = 0.3f; // �����������ȴʱ�䣨�룩�����������

    private int currentIndex = 0;
    private Vector3 targetPosition;
    private float lastScrollTime = -999f; // �ϴι����������ʱ��

    void Start()
    {
        if (anchors.Length > 0)
        {
            // ��ʼ��׼��һ��ê�㣨���·���
            UpdateTargetPosition();
            transform.position = targetPosition;
        }
    }

    void Update()
    {
        HandleScrollInput();
        SmoothMove();
    }

    void HandleScrollInput()
    {
        // ���������ȴʱ��
        if (Time.time - lastScrollTime < scrollCooldown)
        {
            return; // �ڳ���ʱ�����ڣ������������
        }

        // Unity ��׼�����Ϲ�Ϊ�������¹�Ϊ��
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        // ֻҪ�м����������ͻ�����һ��ê��
        if (Mathf.Abs(scroll) > 0.001f)
        {
            if (scroll > 0)
            {
                // ���Ϲ���ǰ�����������Ĺؿ�
                currentIndex = Mathf.Clamp(currentIndex + 1, 0, anchors.Length - 1);
            }
            else
            {
                // ���¹������ص������Ĺؿ�
                currentIndex = Mathf.Clamp(currentIndex - 1, 0, anchors.Length - 1);
            }

            UpdateTargetPosition();
            lastScrollTime = Time.time; // ��¼���ε�������ʱ��
        }
    }

    void UpdateTargetPosition()
    {
        // ȡ��Ŀ��ê��λ�ã�������������� Z ��ƫ�ƣ�2D ͨ��Ϊ -10��
        Vector3 anchorPos = anchors[currentIndex].position;
        targetPosition = new Vector3(transform.position.x, anchorPos.y, transform.position.z);
    }

    void SmoothMove()
    {
        // ʹ�� Lerp ����ƽ���ƶ�
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothSpeed);
    }
}