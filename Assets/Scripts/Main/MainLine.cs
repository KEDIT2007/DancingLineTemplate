using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEditor;
using System.Linq;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class MainLine : MonoBehaviour, IResetable
{
    #region 枚举类型
    public enum LINE_FACING
    {
        FORWARD,
        RIGHT
    }
    public enum LINE_EVENT
    {
        IMMEDIATE,
        TURN,
        DIE,
        START,
        GROUND,
        OFFGROUND
    }
    #endregion

    #region 事件
    //开始事件
    public delegate void StartEventHandler(object sender, LineEventArgs e);
    public event StartEventHandler LineStart;
    //转向事件
    public delegate void TurnEventHandler(object sender, LineEventArgs e);
    public event TurnEventHandler LineTurn;
    //落地事件
    public delegate void GroundEventHandler(object sender, LineEventArgs e);
    public event GroundEventHandler LineGround;
    //离地事件
    public delegate void OffGroundEventHandler(object sender, LineEventArgs e);
    public event OffGroundEventHandler LineOffGround;
    //死亡事件
    public delegate void DieEventHandler(object sender, LineEventArgs e);
    public event DieEventHandler LineDie;
    #endregion

    #region 静态变量（一般用于设置）
    public static List<KeyCode> turnKeys = new List<KeyCode> { KeyCode.Space };
    #endregion

    #region 允许玩家自定义的变量
    [Header("线开始时的朝向")] public MainLine.LINE_FACING forward;
    [Header("线的速度(每秒所过的单位)")] public float speed = 2.0f;
    [Header("落地粒子")] public GameObject groundParticle;
    [Header("死亡粒子")] public DieCube dieParticle;
    [Header("死亡音效")] public AudioClip dieSound;
    [Header("线尾")] public GameObject LineTail;
    [Header("碰撞箱大小")] public Vector2 ColliderSize = new Vector2(0.8f, 0.8f);
    [Header("允许生成掉落粒子的时间")] public float FallTime = 0.25f;
    [Header("线尾上限")] public uint maxTailCount = 150;
    [Header("线的层级名称")] public string LineLayerName = "Line";
    [Header("不与线碰撞的层级名称")] public string NotColWithLineLayerName = "NotColWithLine";
    [Header("触发器的层级名称")] public string TriggerLayerName = "Trigger";
    #endregion

    #region 游戏中自动赋值的变量
    private float startTime = 0;
    private float fallTime = 0;
    private Vector3 startPosition = Vector3.zero;
    private Vector3 lastTurnPosition = Vector3.zero;
    private Quaternion startRotation = Quaternion.identity;
    private BoxCollider _collider;
    private bool isStart = false;
    private List<Collider> collidedObj = new List<Collider>();
    private GameObject lastGroundParticle;
    private DieCube lastDieParticle;
    private GameObject lastLineTail;
    private Queue<GameObject> LineTails = new Queue<GameObject>();
    private bool isStop = false;
    private bool keyDown;
    private int ExtraDirection = 0;
    #endregion

    #region 游戏中公开的自动赋值的变量
    public float time { get; private set; } = 0;
    public bool isDead { get; private set; } = false;
    public MainLine.LINE_FACING nowForward { get; private set; }
    public bool isGrounded
    {
        get
        {
            return (collidedObj.Count == 0) ? false : true;
        }
    }
    #endregion

    void Start()
    {
        #region 生成落地判定用碰撞箱
        BoxCollider collider = GetComponent<BoxCollider>();
        if (collider != null)
        {
            collider.size = new Vector3(ColliderSize.x, collider.size.y, ColliderSize.y);
        }
        _collider = new GameObject("LineCollider", typeof(BoxCollider)).GetComponent<BoxCollider>();
        _collider.transform.rotation = Quaternion.identity;
        _collider.transform.SetParent(transform);
        _collider.transform.localPosition = new Vector3(0, -transform.localScale.y / 2 - 0.001f, 0);
        _collider.transform.localScale = new Vector3(
            ColliderSize.x,
            0,
            ColliderSize.y);
        _collider.isTrigger = true;
        #endregion

        #region 检测是否有有效的EventSystem
        if (EventSystem.current == null)
        {
            Debug.LogError("场景中必须要有UI！");

            #if UNITY_EDITOR
                EditorApplication.isPlaying = false;
            #endif

            #if UNITY_STANDALONE_WIN
                Application.Quit();
            #endif
            
            Destroy(this);
        }
        #endregion

        #region 添加默认事件
        LineStart += lineStart;
        LineGround += lineGround;
        LineOffGround += lineOffGround;
        LineDie += lineDie;
        #endregion

        #region 初始化变量
        lastTurnPosition = transform.position;
        #endregion

    }

    protected virtual void Update()
    {
        //Debug.Log(this.isGrounded);
        //Debug.Log(this.isGrounded.ToString() + this.nowForward.ToString());
        #region 点击触发开始/转向事件
        KeyDown();
        #endregion

        #region 移动 更新线的长度、位置
        if (isStart && !isStop)
        {
            time += Time.deltaTime;
            if (lastLineTail != null)
            {
                UpdateTail();
            }
            transform.Translate(GetDirection(nowForward, ExtraDirection) * speed * Time.deltaTime);
            UpdateFalling();
        }
        #endregion

    }

    #region 静态方法
    public static void SceneReset()
    {
        var objs =
                from obj in FindObjectsOfType<GameObject>()
                where obj.GetComponent<IResetable>() != null
                from comp in obj.GetComponents<IResetable>()
                select comp;
        var first = objs.Where(obj => obj.GetType().GetMethod("ObjectReset").GetCustomAttributes(false).Any(
            attri => attri.GetType() == typeof(FirstCallWhenResetAttribute)));
        var last = objs.Where(obj => obj.GetType().GetMethod("ObjectReset").GetCustomAttributes(false).Any(
            attri => attri.GetType() == typeof(LastCallWhenResetAttribute)));
        var rest = objs.Where(obj => obj.GetType().GetMethod("ObjectReset").GetCustomAttributes(false).All(
            attri => attri.GetType() != typeof(LastCallWhenResetAttribute) &&
                attri.GetType() != typeof(FirstCallWhenResetAttribute)));
        first.ToList().ForEach(obj => obj.ObjectReset());
        rest.ToList().ForEach(obj => obj.ObjectReset());
        last.ToList().ForEach(obj => obj.ObjectReset());
    }
    public static int AddStartEvent(MainLine line, StartEventHandler e)
    {
        if (e == line.lineStart)
        {
            return -1;
        }
        try
        {
            if (!line.LineStart.GetInvocationList().Contains(e))
            {
                line.LineStart += e;
                return 0;
            }
            else
            {
                return 1;
            }
        }
        catch
        {
            return 2;
        }
    }
    public static int RemoveStartEvent(MainLine line, StartEventHandler e)
    {
        if (e == line.lineStart)
        {
            return -1;
        }
        try
        {
            if (line.LineStart.GetInvocationList().Contains(e))
            {
                line.LineStart -= e;
                return 0;
            }
            else
            {
                return 1;
            }
        }
        catch
        {
            return 2;
        }
    }
    public static int AddTurnEvent(MainLine line, TurnEventHandler e)
    {
        if (e == line.lineTurn)
        {
            return -1;
        }
        try
        {
            if (line.LineTurn == null || !line.LineTurn.GetInvocationList().Contains(e))
            {
                line.LineTurn += e;
                return 0;
            }
            else
            {
                return 1;
            }
        }
        catch
        {
            return 2;
        }
    }
    public static int RemoveTurnEvent(MainLine line, TurnEventHandler e)
    {
        if (e == line.lineTurn)
        {
            return -1;
        }
        try
        {
            if (line.LineTurn == null || line.LineTurn.GetInvocationList().Contains(e))
            {
                line.LineTurn -= e;
                return 0;
            }
            else
            {
                return 1;
            }
        }
        catch
        {
            return 2;
        }
    }
    public static int AddGroundEvent(MainLine line, GroundEventHandler e)
    {
        if (e == line.lineGround)
        {
            return -1;
        }
        try
        {
            if (!line.LineGround.GetInvocationList().Contains(e))
            {
                line.LineGround += e;
                return 0;
            }
            else
            {
                return 1;
            }
        }
        catch
        {
            return 2;
        }
    }
    public static int RemoveGroundEvent(MainLine line, GroundEventHandler e)
    {
        if (e == line.lineGround)
        {
            return -1;
        }
        try
        {
            if (line.LineGround.GetInvocationList().Contains(e))
            {
                line.LineGround -= e;
                return 0;
            }
            else
            {
                return 1;
            }
        }
        catch
        {
            return 2;
        }
    }
    public static int AddOffGroundEvent(MainLine line, OffGroundEventHandler e)
    {
        if (e == line.lineOffGround)
        {
            return -1;
        }
        try
        {
            if (!line.LineOffGround.GetInvocationList().Contains(e))
            {
                line.LineOffGround += e;
                return 0;
            }
            else
            {
                return 1;
            }
        }
        catch
        {
            return 2;
        }
    }
    public static int RemoveOffGroundEvent(MainLine line, OffGroundEventHandler e)
    {
        if (e == line.lineOffGround)
        {
            return -1;
        }
        try
        {
            if (line.LineOffGround.GetInvocationList().Contains(e))
            {
                line.LineOffGround -= e;
                return 0;
            }
            else
            {
                return 1;
            }
        }
        catch
        {
            return 2;
        }
    }
    public static int AddDieEvent(MainLine line, DieEventHandler e)
    {
        if (e == line.lineDie)
        {
            return -1;
        }
        try
        {
            if (!line.LineDie.GetInvocationList().Contains(e))
            {
                line.LineDie += e;
                return 0;
            }
            else
            {
                return 1;
            }
        }
        catch
        {
            return 2;
        }
    }
    public static int RemoveDieEvent(MainLine line, DieEventHandler e)
    {
        if (e == line.lineDie)
        {
            return -1;
        }
        try
        {
            if (line.LineDie.GetInvocationList().Contains(e))
            {
                line.LineDie -= e;
                return 0;
            }
            else
            {
                return 1;
            }
        }
        catch
        {
            return 2;
        }
    }
    public static int RunLineDie(MainLine line, bool doStop)
    {
        try
        {
            line.isDead = true;
            line.isStop = doStop;
            line.LineDie(line.gameObject, new LineEventArgs(line, line.transform.position, line.nowForward));
            return 0;
        }
        catch
        {
            return -1;
        }
    }
    public static Vector3 GetDirection(MainLine.LINE_FACING facing, int extraDirection)
    {
        int extraD;
        if (extraDirection >= 0)
        {
            extraD = extraDirection % 4;
        }
        else
        {
            extraD = 4 - Mathf.Abs(extraDirection) % 4;
        }
        Vector3 dir;
        switch(extraD)
        {
            case 1:
                dir = (facing == LINE_FACING.FORWARD) ? Vector3.right : Vector3.back;
                return dir;
            case 2:
                dir = (facing == LINE_FACING.FORWARD) ? Vector3.back : Vector3.left;
                return dir;
            case 3:
                dir = (facing == LINE_FACING.FORWARD) ? Vector3.left : Vector3.forward;
                return dir;
            default:
                dir = (facing == LINE_FACING.FORWARD) ? Vector3.forward : Vector3.right;
                return dir;
        }
    }
    public static int GetDirectionAxis(MainLine.LINE_FACING facing, int extraDirection)
    {
        Vector3 dir = GetDirection(facing, extraDirection);
        float[] dirAxis = new float[3] { dir[0], dir[1], dir[2] };
        for (int i = 0; i < 3; i++)
        {
            if (dirAxis[i] != 0)
            {
                return i;
            }
        }
        throw new NullReferenceException();
    }
    public static void AddExtraDirection(MainLine line, int value)
    {
        line.ExtraDirection += value;
    }
    #endregion

    #region 默认的事件

    [FirstCallWhenReset]
    public void ObjectReset()
    {
        if (isStart)
        {
            ExtraDirection = 0;
            GetComponent<Rigidbody>().isKinematic = false;
            isDead = false;
            nowForward = forward;
            isStart = false;
            isStop = false;
            time = startTime;
        }
        GameObject.FindGameObjectsWithTag("JumpEffect").ToList().ForEach(obj => Destroy(obj));
        GameObject.FindGameObjectsWithTag("DieEffect").ToList().ForEach(obj => Destroy(obj));
        LineTails.ToList().ForEach(x => Destroy(x));
        if (LineTails.Count != 0)
        {
            LineTails.Clear();
        }
        transform.position = startPosition;
        collidedObj.Where(col => col == null).ToList().ForEach(obj => collidedObj.Remove(obj));
        enabled = true;
        transform.rotation = startRotation;
        lastTurnPosition = startPosition;
        LineStart = lineStart;
        LineGround = lineGround;
        LineOffGround = lineOffGround;
        LineDie = lineDie;
    }

    public void Turn()
    {
        LineEventArgs e = new LineEventArgs(this, transform.position, nowForward) { ForceTurn = true };
        this.LineTurn?.Invoke(this, e);
        lineTurn(this, e);
    }

    protected virtual void lineStart(object sender, LineEventArgs e)
    {
        isStart = true;
        NewLineTail(true);
        startPosition = e.Position;
        startRotation = e.Line.transform.rotation;
        startTime = time;
        //Debug.Log(e.Position.ToString() + e.Line.name + e.StartForward.ToString());
    }
    public virtual void lineTurn(object sender, LineEventArgs e)
    {
        if ((e.AllowTurn || e.ForceTurn) && !e.ChangeMove)
        {
            nowForward = (nowForward == LINE_FACING.FORWARD) ? LINE_FACING.RIGHT : LINE_FACING.FORWARD;
            NewLineTail(true);
        }
        //Debug.Log(e.Position.ToString() + e.Line.name + e.TurningForward.ToString());
    }
    protected virtual void lineGround(object sender, LineEventArgs e)
    {
        if (groundParticle != null && fallTime >= FallTime)
        {
            lastGroundParticle = Instantiate(groundParticle, transform.position, Quaternion.identity);
            lastGroundParticle.gameObject.tag = "JumpEffect";
            Destroy(lastGroundParticle, 5f);
        }
        fallTime = 0;
        NewLineTail(true);
        //Debug.Log(e.Position.ToString() + e.Line.name);
    }
    protected virtual void lineOffGround(object sender, LineEventArgs e)
    {
        NewLineTail(false);
        //Debug.Log(e.Position.ToString() + e.Line.name);
    }
    protected virtual void lineDie(object sender, LineEventArgs e)
    {
        isDead = true;
        if (dieSound != null && isStop)
        {
            AudioSource.PlayClipAtPoint(dieSound, e.Position);
        }
        if (dieParticle != null && isStop)
        {
            lastDieParticle = Instantiate(dieParticle, e.Position, Quaternion.identity);
            lastDieParticle.gameObject.tag = "DieEffect";
            lastDieParticle.Show(e.Position);
            Destroy(lastDieParticle.gameObject, 10f);
        }
        if (isStop)
        {
            GetComponent<Rigidbody>().isKinematic = true;
        }
    }
    public virtual void NewLineTail(bool Grounded)
    {
        lastTurnPosition = this.transform.position;
        if (!Grounded)
        {
            lastLineTail = null;
        }
        else if (LineTail != null)
        {
            GameObject newTail;
            if (LineTails.Count >= maxTailCount)
            {
                newTail = LineTails.Dequeue();
            }
            else
            {
                newTail = Instantiate(LineTail, transform.position, transform.rotation);
            }
            newTail.transform.rotation = transform.rotation;
            LineTails.Enqueue(newTail);
            lastLineTail = newTail;
            //Debug.Log(this.lastLineTail.name);
        }
    }
    protected virtual void UpdateTail()
    {
        //Debug.Log("Mid: " + ((this.lastTurnPosition + this.transform.position) / 2).ToString());
        lastLineTail.transform.position = (lastTurnPosition + transform.position) / 2;
        Vector3 scale = transform.lossyScale;
        int dirAxis = GetDirectionAxis(nowForward, ExtraDirection);
        scale[dirAxis] = Mathf.Abs(GetDirection(nowForward, ExtraDirection)[dirAxis]) * Vector3.Distance(lastTurnPosition, transform.position) + transform.lossyScale[dirAxis];
        //Debug.Log("Scale: " + scale[dirAxis]);
        lastLineTail.transform.localScale = scale;
    }
    private void UpdateFalling()
    {
        if (isGrounded)
        {
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            float[] linePosition = new float[2] { transform.position[0], transform.position[2] };
            float[] tailPosition = new float[2] { lastTurnPosition[0], lastTurnPosition[2] };
            for (int i = 0; i < 2; i++)
            {
                if (linePosition[i] == tailPosition[i])
                {
                    break;
                }
                else if (i == 1)
                {
                    if (Mathf.Abs(linePosition[i] - tailPosition[i]) >= (ColliderSize[0] / 2))
                    {
                        NewLineTail(true);
                        lastTurnPosition = transform.position;
                    }
                    else
                    {
                        NewLineTail(false);
                    }
                }
            }
            if (Vector3.Distance(transform.position, lastTurnPosition) >= 1f && lastLineTail == null)
            {
                lastTurnPosition = transform.position;
                NewLineTail(true);
            }
        }
        else
        {
            fallTime += Time.deltaTime;
        }
    }
    private void KeyDown()
    {
        bool turn = false;
        turn = MainLine.turnKeys.Any(key => Input.GetKey(key));
        if (Input.GetMouseButton(0))
        {
            turn = true;
        }
        if (!isStart && turn && !EventSystem.current.IsPointerOverGameObject() && !keyDown)
        {
            keyDown = true;
            LineStart(
                gameObject,
                new LineEventArgs(this, transform.position, forward)
                );
        }
        else if (turn
            && !EventSystem.current.IsPointerOverGameObject()
            && !keyDown
            && isGrounded && !isDead)
        {
            keyDown = true;
            LineEventArgs e = new LineEventArgs(this, transform.position, nowForward);
            LineTurn?.Invoke(this, e);
            lineTurn(this, e);
        }
        else if (!turn)
        {
            keyDown = false;
        }
        else
        {
            keyDown = true;
        }
    }
    #endregion

    public void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer(LineLayerName) && other.gameObject.layer != LayerMask.NameToLayer(TriggerLayerName))
        {
            if (other.gameObject.CompareTag("Wall"))
            {
                isStop = true;
                LineDie(gameObject, new LineEventArgs(this, transform.position, nowForward));
            }
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        //Debug.Log(LayerMask.LayerToName(other.gameObject.layer) + "In");
        if (other.gameObject.layer != LayerMask.NameToLayer(LineLayerName) 
            && other.gameObject.layer != LayerMask.NameToLayer(TriggerLayerName)
            && other.gameObject.layer != LayerMask.NameToLayer(NotColWithLineLayerName)
            && !other.gameObject.CompareTag("Wall")
            && !other.gameObject.CompareTag("DieEffect")
            && !other.gameObject.CompareTag("JumpEffect"))
        {
            if (collidedObj.Count == 0 && isStart)
            {
                LineGround(gameObject,
                    new LineEventArgs(this,
                        transform.position, nowForward));
            }
            collidedObj.Add(other);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer(TriggerLayerName) && other.CompareTag("OutmapTrigger"))
        {
            LineDie(gameObject, new LineEventArgs(this, transform.position, nowForward));
        }
        //Debug.Log(this.collidedObj.Count);
    }

    public void OnTriggerExit(Collider other)
    {
        //Debug.Log(LayerMask.LayerToName(other.gameObject.layer) + "Out");
        if (collidedObj.Contains(other))
        {
            collidedObj.Remove(other);
        }
        if (collidedObj.Count == 0)
        {
            LineOffGround(gameObject, new LineEventArgs(this, transform.position, nowForward));
        }
        //Debug.Log(this.collidedObj.Count);
    }

}

#region 事件参数
//参数父类
public class LineEventArgs : EventArgs
{
    public MainLine Line;
    public Vector3 Position;
    public MainLine.LINE_FACING Facing;
    public bool AllowTurn = true;
    public bool ForceTurn = false;
    public bool ChangeMove = false;
    public LineEventArgs(MainLine line, Vector3 position, MainLine.LINE_FACING facing)
    {
        Line = line;
        Position = position;
        Facing = facing;
    }
}
#endregion

#region 接口
//可重置
public interface IResetable
{
    void ObjectReset();
}
#endregion

#region 标签
/// <summary>
/// 在重置时最后被调用
/// </summary>
sealed class LastCallWhenResetAttribute : Attribute
{

}
/// <summary>
/// 在重置时最先被调用
/// </summary>
sealed class FirstCallWhenResetAttribute : Attribute
{

}
#endregion