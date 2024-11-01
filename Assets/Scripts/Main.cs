using UnityEngine;
using System.Collections;

public struct TetrixShape // ��������� ��� �������� ������������ ������
{
    public float x, y; // ���������� ������
    public int blocks_count; // ���������� ������
    public float[,] block_pos; // ������ �������� ������ ������������ ������. [dX, dY]
    public GameObject[] block_sprite; // ������ �������� - �������� ��� ������
    public int[] block_id; // ������ ��������������� �������� ������ �� ������������ ������� �������� sprite_block
    public float nextTime; // ����� ���������� ������ ������
    public bool can_rotate; // ����� �� ������� ������
    public bool can_rotate_sprite; // ����� �� ������� ������� ������
    public float speed; // �������� ������� ������ (������ �� �����)
    public bool drop_shape; // ������� ����� ������
}
public class Main : MonoBehaviour
{
    public LevelManager levelManager; // ������ �� ������, ���������� �������� ����

    public static bool stop_game = true; // ���������� �� �������� �����? ����� � ����-����

    //private int y_check; // ����� ����������� �����.
    private GameObject[,] space_cells_sprite; // ��������� ������ ��������-�������� �������
    public string[,] space_cells_status; // ��������� ������ �������� ����� ������� (��������� �������� ��� ��������)
    [SerializeField] Sprite sprite_background; // ������ ������ ������ �������
    [SerializeField] GameObject prefabSmoke; // ������ ����
    [SerializeField] GameObject prefabFlash; // ������ ������� ������

    private TetrixShape[] shape = new TetrixShape[2]; // ������ �����
    public int N_shape; // ����� ������, �������������� � ������ ������
    public Sprite[] sprite_block; // ����� ������ �������� ������ (��� ��������). ����������� � ����������

    // �����
    [SerializeField] AudioSource sound_pong; // �������� ������
    [SerializeField] AudioSource sound_crash; // �������� �����
    [SerializeField] AudioSource sound_boom; // ����� ������ �����

    private void Awake()
    {
        stop_game = true; // ������������� ����������� ����������
    }
    public void PrepareGame() // ���������� ����
    {
        Debug.ClearDeveloperConsole(); // ������� �������
        UnityEngine.Random.InitState(System.DateTime.Now.Millisecond); // ������������� ���������������� ���������� ����� �� �������� ���� � �� �� ������

        // ��������� ������ ���������, ������� � ���� ���� ����� ������ ���� ������, ��� ��������. ��������� � ��������� ������� space_cells_sprite[,]
        // ������������ ��������� ����������� ������ space_cells_status[,] � ��������� ��������� �����, ������������ � ���� ������
        space_cells_sprite = new GameObject[16, 41]; // ��������� ������ ��������-�������� ������ ������
        space_cells_status = new string[16, 41]; // ��������� ������ �������� ����� �������
        for (int j = 1; j <= 40; j++) // �� Y
        {
            for (int i = 1; i < 16; i++) // �� X
            {
                space_cells_sprite[i, j] = new GameObject("Cell(" + i + "," + j + ")");
                // ��������� ��������� SpriteRenderer � ������
                SpriteRenderer spriteRenderer = space_cells_sprite[i, j].AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = sprite_background;
                // ������������� ������
                space_cells_sprite[i, j].transform.position = new Vector3(i * 1f, j * 1f, 0);
                spriteRenderer.sortingOrder = 1; // ������� ���� - ����� ��������, �� �� ��������
                // ���������� ������ �����
                space_cells_status[i, j] = "";
            }
        }
        stop_game = false; // ������� � �����
    }


    public void RenderNewShape() // ���������� ����� ������ �� �����
    {
        int block_x; // ������� ����� ������ � �������. ����� ����� ��� ��������� � ������� ��������� ����� �������
        int block_y;

        // ��������, ���� �� �� ����� ������ ������ ������
        // ��� ��������� ����� ��������� ����� ������ �����
        bool have_place = false; // ���������� �� ����� ��� ����� ������
        float try_y = shape[N_shape].y; // �������������� ������ ����� ������
        shape[N_shape].y = 100f; // ���� �������� ������ ����, ����� ����� �� ��������� � �������� �������
        while (!have_place) // ����� ��������� �������������� ����� ������ ���� ��� ����� �� ������� �������� ������
        {
            have_place = true; // �����������, ��� ����� ��������
            for (int i = 0; i < shape[N_shape].blocks_count; i++) // ���� �� ������ ������
            {
                block_x = (int)(shape[N_shape].x + shape[N_shape].block_pos[i, 0]); // ������� ����� ������
                block_y = (int)(try_y + shape[N_shape].block_pos[i, 1]);
                have_place = (ShapeInCell(block_x, block_y) == -1 || ShapeInCell(block_x, block_y) == N_shape); // �� ������ �� ����� ��� ����� ���� ������ �������?
                if (!have_place) // ���� ����� ��� ������ ������ ������ �������
                { try_y++; break; } // ��������� ��� ������ ����� �� ���� ����� � ������� �� ����� ��� ��������� ��������
            }
        }
        shape[N_shape].y = try_y; // � ������� ��� ����� ������� ������������

        // ��������� ��������-�������� ��� ������������ ������
        for (int i = 0; i < shape[N_shape].blocks_count; i++) // ���� �� ������ ������
        {
            // ��������, � �� �������� �� ��?
            block_x = (int)(shape[N_shape].x + shape[N_shape].block_pos[i, 0]); // ���������� ����� ������. ����� ����� ��� ��������� � ������� ��������� ������
            block_y = (int)(shape[N_shape].y + shape[N_shape].block_pos[i, 1]);
            if (space_cells_status[block_x, block_y] != "") { levelManager.GameOver(); } // ��������, ������ ������� ��� �� �����

            shape[N_shape].block_sprite[i] = new GameObject("Shape_Cell(" + i + ")"); // ������ ����� ������ (�������-�������)
            //��������� ��������� SpriteRenderer � �����
            SpriteRenderer spriteRenderer = shape[N_shape].block_sprite[i].AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite_block[shape[N_shape].block_id[i]]; // �������� ������ ����� �� �������
            spriteRenderer.sortingOrder = 2; // ������� ���� - ����� �������� �������
        }
        // ������������� ����� (��������� �� ����� � ������������ � �������� �������� shape[N_shape].block_pos[] + ����� ������)
        for (int i = 0; i < shape[N_shape].blocks_count; i++)
        {
            shape[N_shape].block_sprite[i].transform.position = new Vector3(shape[N_shape].x + shape[N_shape].block_pos[i, 0], shape[N_shape].y + shape[N_shape].block_pos[i, 1], 0);
        }
        shape[N_shape].drop_shape = false;
        shape[N_shape].nextTime = Time.time + shape[N_shape].speed; // ����� ���������� ��������� ������ �������� ����� �������
        N_shape = -1; // ������ � ������� ��������, ��������� �
    }
    private int ShapeInCell(int x, int y) // ����������� id ������, ����������� � ������. "-1" - ���� � ������ ��� �����
    {
        for (int check_shape = 0; check_shape <= 1; check_shape++) // ������������� ��� ������ �� �������
        {
            for (int i = 0; i < shape[check_shape].blocks_count; i++) // ���������� ��� ����� ��������������� ������
            {
                if (shape[check_shape].x + shape[check_shape].block_pos[i, 0] == x &&
                    shape[check_shape].y + shape[check_shape].block_pos[i, 1] == y) // ���� ������� ������-��������� ��������� � �������� ����� ������, �� ���������� id ��� ������
                { return check_shape; }
            }
        }
        return -1; // � ������ ��� �����
    }

    void Update() // ������ ���
    {
        if (stop_game) { return; } // ���� ���� �����������, ��������

        // ������� ������ 
        if (Input.GetKeyDown(KeyCode.A)) { N_shape = 0; MoveAside(-1); } // ������� ������ �����
        if (Input.GetKeyDown(KeyCode.D)) { N_shape = 0; MoveAside(1); } // ������� ������ ������
        if (Input.GetKeyDown(KeyCode.S) && shape[0].blocks_count < 16) { shape[0].drop_shape = true; } // ����� ������ ����
        if (Input.GetKeyDown(KeyCode.W) && shape[0].can_rotate) { N_shape = 0; RotateShape(); } // �������� ������ �� �������
        if (Input.GetKeyDown(KeyCode.LeftArrow)) { N_shape = 1; MoveAside(-1); } // ������� ������ �����
        if (Input.GetKeyDown(KeyCode.RightArrow)) { N_shape = 1; MoveAside(1); } // ������� ������ ������
        if (Input.GetKeyDown(KeyCode.DownArrow) && shape[1].blocks_count < 16) { shape[1].drop_shape = true; } // ����� ������ ����
        if (Input.GetKeyDown(KeyCode.UpArrow) && shape[1].can_rotate) { N_shape = 1; RotateShape(); } // �������� ������ �� �������

        // �������� ����� �� ��������
        N_shape = 0; // �������� ��������� ������ ������
        LowerShape();
        N_shape = 1;
        LowerShape(); // �������� ��������� ������ ������
        N_shape = -1;
    }

    // ���������� � ��������� ������
    public void LeftLeftButton_press() { if (!stop_game) { N_shape = 0; MoveAside(-1); } } // ������� ����� ������ �����
    public void LeftRightButton_press() { if (!stop_game) { N_shape = 0; MoveAside(1); } } // ������� ����� ������ ������
    public void LeftUpButton_press() { if (!stop_game && shape[0].can_rotate) { N_shape = 0; RotateShape(); } } // �������� ����� ������ �� �������
    public void LeftDownButton_press() { if (!stop_game && shape[0].blocks_count < 16) { shape[0].drop_shape = true; } } // ����� ����� ������
    public void RightLeftButton_press() { if (!stop_game) { N_shape = 1; MoveAside(-1); } } // ������� ������ ������ �����
    public void RightRightButton_press() { if (!stop_game) { N_shape = 1; MoveAside(1); } } // ������� ������ ������ ������
    public void RightUpButton_press() { if (!stop_game && shape[1].can_rotate) { N_shape = 1; RotateShape(); } } // �������� ������ ������ �� �������
    public void RightDownButton_press() { if (!stop_game && shape[1].blocks_count < 16) { shape[1].drop_shape = true; } } // ����� ������ ������

    private void MoveAside(int delta) // �������� ������ � �������. delta - ��������, ���� -1 ��� �����, ���� 1 �� ������
    {
        // ���������, ���� �� ����� ����� ������. ���� ���, �� ��������� ������ ������� � ������ �� ��������
        for (int i = 0; i < shape[N_shape].blocks_count; i++) // ���� �� ������ ������
        {
            int block_x = (int)(shape[N_shape].x + shape[N_shape].block_pos[i, 0]) + delta; // ���������� ������ � ������� ����� �������� ���� ������. ����� ����� ��� ��������� � ������� ��������� ������
            int block_y = (int)(shape[N_shape].y + shape[N_shape].block_pos[i, 1]);
            if (block_x == 0 || block_x == 16) { return; } // ������� ������, ������ �����
            if (space_cells_status[block_x, block_y] != "") { return; } // ���� �� ������ ������ ����� � �����-�� ���� �������
            if (ShapeInCell(block_x, block_y) != -1 && ShapeInCell(block_x, block_y) != N_shape) { return; } // ������� ������, ������ ������ ������
        }
        // ��� �������� ��������, ����� �� ������. ��������
        shape[N_shape].x = shape[N_shape].x + (float)delta;
        for (int i = 0; i < shape[N_shape].blocks_count; i++)
        {
            float new_X = shape[N_shape].x + shape[N_shape].block_pos[i, 0];
            float new_Y = shape[N_shape].y + shape[N_shape].block_pos[i, 1];
            shape[N_shape].block_sprite[i].transform.position = new Vector3(new_X, new_Y, 0); // ����������� �����
        }
    }
    private void RotateShape() // �������� ������. 
    {
        // ������ �������� - ��� �������� ������ ������ ������� �������� �� X � Y

        // ��������, ���� �� ����� ��� ������ ����� ���������
        for (int i = 0; i < shape[N_shape].blocks_count; i++) // ���� �� ������ ������
        {
            int block_x = (int)(shape[N_shape].x + shape[N_shape].block_pos[i, 1]); // ���������� ������ � ������� ����� �������� ���� ������. ����� ����� ��� ��������� � ������� ��������� ������
            int block_y = (int)(shape[N_shape].y - shape[N_shape].block_pos[i, 0]);
            if (block_x <= 0 || block_x >= 16 || block_y <= 0) { return; } // ����� ������ ��������
            if (space_cells_status[block_x, block_y] != "") { return; } // ���� �� ������ ������ ��� �������� �������� �� ������� �����
            if (ShapeInCell(block_x, block_y) != -1 && ShapeInCell(block_x, block_y) != N_shape) { return; } // �������� ������ ������ ������
        }
        // ��� �������� ��������, ����� �� ������. ������
        for (int i = 0; i < shape[N_shape].blocks_count; i++)
        {
            // ���������� �������� ���������� ����� ������. X � Y ������ �������, ��� ����� ����� ��������� ����������, � � ����� ��� ���������
            float new_X = shape[N_shape].block_pos[i, 1];
            float new_Y = -1f * shape[N_shape].block_pos[i, 0];
            // ����� ���������� ����� ������
            shape[N_shape].block_pos[i, 0] = new_X;
            shape[N_shape].block_pos[i, 1] = new_Y;

            // ����������� � ������� �������
            shape[N_shape].block_sprite[i].transform.position = new Vector3(shape[N_shape].x + new_X, shape[N_shape].y + new_Y, 0); // �����������
            if (shape[N_shape].can_rotate_sprite) { shape[N_shape].block_sprite[i].transform.Rotate(0, 0, -90); } // ���� ������� ������ ��������������. ��� ��� ������� ������
            else if (shape[N_shape].block_id[i] > 25)
            {
                // ���� ��� ��� �������� ������ 2x2. ������ - ������������� id � ������� ������ �������, ����� ������� �� �������� ���������� � ������������ ���������
                int shape_id = shape[N_shape].block_id[i]; // id ������� ���� ���������� ����� 2x2
                if ((shape_id - 1) % 4 == 0) { shape_id = shape_id - 3; }
                else { shape_id = shape_id + 1; } // ������ id �������, "���������" ��� ������� ������ �������
                shape[N_shape].block_id[i] = shape_id;
                shape[N_shape].block_sprite[i].GetComponent<SpriteRenderer>().sprite = sprite_block[shape_id]; // ������ ������ ����
            }
        }
    }

    
    
    private void LowerShape() // ����� ������ ���� �� ������� 
    {
        if (shape[N_shape].drop_shape) { shape[N_shape].nextTime = Time.time; } // ���� ��� ����� ������, �� ���������� ������� �������
        if (Time.time >= shape[N_shape].nextTime)
        {
            // ��������� ��������, ������� �� ������ �� ������ ������?
            for (int i = 0; i < shape[N_shape].blocks_count; i++) // ���� �� ������ ������
            {
                int check_x = (int)(shape[N_shape].x + shape[N_shape].block_pos[i, 0]); // X ���������� ������ ��� ������
                int check_y = (int)(shape[N_shape].y + shape[N_shape].block_pos[i, 1] - 1); // Y ���������� ������ ��� ������
                if (ShapeInCell(check_x, check_y) != -1 && ShapeInCell(check_x, check_y) != N_shape) // ��� ���� ������ ������,
                { shape[N_shape].nextTime = Time.time + shape[N_shape].speed / 2; return; } // ������������ ���� ���� ������. �������
            }
            // �������� ���������� ���
            if (IsShapeDown()) { FixShape(); return; } // ���� ������ �����, ��������� � ����� � ������ �������, ������� � � ������ �����. �� ��� � FixShape, ������ �������
            //CheckDown(); // ����� �������, ������� �������� ����� �� ������. � ���� �����, �� ������ �.
            //f (N_shape == -1) { return; } // ������ ������� � ������ � ���������� ��� �������������. ������� �� �������
            // ������ �� �� ��� �� �����
            // ��� �������� ��������, ����� �������� ������
            shape[N_shape].y--; // ����� ���� ������ ������ ������
            for (int i = 0; i < shape[N_shape].blocks_count; i++) // ������� �������� ������� ������ �� ���� ����
            {
                shape[N_shape].block_sprite[i].transform.position = new Vector3(shape[N_shape].x + shape[N_shape].block_pos[i, 0], shape[N_shape].y + shape[N_shape].block_pos[i, 1], 0); // ����� ������� �������
            }
            // ������������� ����� ���������� ��������� ������
            shape[N_shape].nextTime = Time.time + shape[N_shape].speed;
        }

    }
    private bool IsShapeDown() // ��������, ���������� �� ������ �� ��� ��� ������� ������ �������
    {
        for (int i = 0; i < shape[N_shape].blocks_count; i++) // ���� �� ������ ������. �������� �������, ���� �� ��� ��� ������ ����� ��� ��������
        {
            int cell_x = (int)(shape[N_shape].x + shape[N_shape].block_pos[i, 0]); // ������� ������� ������ �������, � ������� ����������� ���� ������. ����� �����.
            int cell_y = (int)(shape[N_shape].y + shape[N_shape].block_pos[i, 1]);
            if (cell_y == 1 || space_cells_status[cell_x, cell_y - 1] != "") // ���� ���� ������ ������ ��� ��� ��� ��� ������� ������
            { return true; } // ���������� ������������� �����
        }
        return false; // ��� ����� �������� ��������, ���������� ������������� ����� - ������ �� �����
    }

    private void FixShape() // ������� ������ ������ � ������ �������
    {        
        sound_pong.Play();
        // �������� ������ ������� �� ������� ��� ������ � �������������
        for (int j = 0; j < shape[N_shape].blocks_count; j++) // ���� �� ������ ������
        {
            int cell_x = (int)(shape[N_shape].x + shape[N_shape].block_pos[j, 0]); //���������� ����� ������. ����� ����� ��� ��������� � ������� ����� ������� space_cells_status
            int cell_y = (int)(shape[N_shape].y + shape[N_shape].block_pos[j, 1]);

            switch (shape[N_shape].block_id[j])
            //��������� id ������� ����� �� ������� ������ � ��������� ��� ��� ������ ������� (��� �������� ����������������). 
            {
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                    space_cells_status[cell_x, cell_y] = "classic";
                    space_cells_sprite[cell_x, cell_y].GetComponent<SpriteRenderer>().sprite = sprite_block[0]; break;
                case 8:
                    space_cells_status[cell_x, cell_y] = "diamond";
                    space_cells_sprite[cell_x, cell_y].GetComponent<SpriteRenderer>().sprite = sprite_block[8];
                    FallDiamond(cell_x, cell_y); break; // ����������� ����� ������� ����� �� ����. ����� �� ��������!
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                case 16:
                    space_cells_status[cell_x, cell_y] = "beton";
                    space_cells_sprite[cell_x, cell_y].GetComponent<SpriteRenderer>().sprite = sprite_block[9]; break;
                case 18:
                    space_cells_status[cell_x, cell_y] = "2x2 -l-d bombBlack";
                    space_cells_sprite[cell_x, cell_y].GetComponent<SpriteRenderer>().sprite = sprite_block[shape[N_shape].block_id[j]]; break;
                case 19:
                    space_cells_status[cell_x, cell_y] = "2x2 -l-u bombBlack";
                    space_cells_sprite[cell_x, cell_y].GetComponent<SpriteRenderer>().sprite = sprite_block[shape[N_shape].block_id[j]]; break;
                case 20:
                    space_cells_status[cell_x, cell_y] = "2x2 -r-u bombBlack";
                    space_cells_sprite[cell_x, cell_y].GetComponent<SpriteRenderer>().sprite = sprite_block[shape[N_shape].block_id[j]]; break;
                case 21:
                    space_cells_status[cell_x, cell_y] = "2x2 -r-d bombBlack";
                    space_cells_sprite[cell_x, cell_y].GetComponent<SpriteRenderer>().sprite = sprite_block[shape[N_shape].block_id[j]];
                    if (shape[N_shape].drop_shape)
                    {
                        int randomLucky = UnityEngine.Random.Range(0, 2);  // ����������� ������������ �����
                        if (randomLucky != 0) { DamageCell2x2(cell_x, cell_y, 1); } // ���� ����� ����������, ��� ����������
                    }
                    break;
                case 22:
                    space_cells_status[cell_x, cell_y] = "2x2 -l-d bombBlack";
                    space_cells_sprite[cell_x, cell_y].GetComponent<SpriteRenderer>().sprite = sprite_block[shape[N_shape].block_id[j] - 4]; break;
                case 23:
                    space_cells_status[cell_x, cell_y] = "2x2 -l-u bombBlack";
                    space_cells_sprite[cell_x, cell_y].GetComponent<SpriteRenderer>().sprite = sprite_block[shape[N_shape].block_id[j] - 4]; break;
                case 24:
                    space_cells_status[cell_x, cell_y] = "2x2 -r-u bombBlack";
                    space_cells_sprite[cell_x, cell_y].GetComponent<SpriteRenderer>().sprite = sprite_block[shape[N_shape].block_id[j] - 4]; break;
                case 25:
                    space_cells_status[cell_x, cell_y] = "2x2 -r-d bombBlack";
                    space_cells_sprite[cell_x, cell_y].GetComponent<SpriteRenderer>().sprite = sprite_block[shape[N_shape].block_id[j] - 4]; break;
                case 26:
                    space_cells_status[cell_x, cell_y] = "2x2 -l-d diamond";
                    space_cells_sprite[cell_x, cell_y].GetComponent<SpriteRenderer>().sprite = sprite_block[shape[N_shape].block_id[j]]; break;
                case 27:
                    space_cells_status[cell_x, cell_y] = "2x2 -l-u diamond";
                    space_cells_sprite[cell_x, cell_y].GetComponent<SpriteRenderer>().sprite = sprite_block[shape[N_shape].block_id[j]]; break;
                case 28:
                    space_cells_status[cell_x, cell_y] = "2x2 -r-u diamond";
                    space_cells_sprite[cell_x, cell_y].GetComponent<SpriteRenderer>().sprite = sprite_block[shape[N_shape].block_id[j]]; break;
                case 29:
                    space_cells_status[cell_x, cell_y] = "2x2 -r-d diamond";
                    space_cells_sprite[cell_x, cell_y].GetComponent<SpriteRenderer>().sprite = sprite_block[shape[N_shape].block_id[j]];
                    FallDiamond2x2(cell_x, cell_y); break; // ����������� ����� ������� ����� �� ����, ���� �� �� �� ���.
                case 50:
                case 54:
                case 58:
                case 62:
                case 66:
                case 70:
                case 74:
                    space_cells_status[cell_x, cell_y] = "2x2 -l-d beton crack-00";
                    space_cells_sprite[cell_x, cell_y].GetComponent<SpriteRenderer>().sprite = sprite_block[30]; // ������ �������������� ������ ����� 2x2
                    break;
                case 51:
                case 55:
                case 59:
                case 63:
                case 67:
                case 71:
                case 75:
                    space_cells_status[cell_x, cell_y] = "2x2 -l-u beton crack-00";
                    space_cells_sprite[cell_x, cell_y].GetComponent<SpriteRenderer>().sprite = sprite_block[31]; // ������ �������������� ������ ����� 2x2
                    break;
                case 52:
                case 56:
                case 60:
                case 64:
                case 68:
                case 72:
                case 76:
                    space_cells_status[cell_x, cell_y] = "2x2 -r-u beton crack-00";
                    space_cells_sprite[cell_x, cell_y].GetComponent<SpriteRenderer>().sprite = sprite_block[32]; // ������ �������������� ������ ����� 2x2
                    break;
                case 53:
                case 57:
                case 61:
                case 65:
                case 69:
                case 73:
                case 77:
                    space_cells_status[cell_x, cell_y] = "2x2 -r-d beton crack-00";
                    space_cells_sprite[cell_x, cell_y].GetComponent<SpriteRenderer>().sprite = sprite_block[33]; // ������ �������������� ������ ����� 2x2
                    break;
                case 78:
                    space_cells_status[cell_x, cell_y] = "2x2 -l-d bombWhite";
                    space_cells_sprite[cell_x, cell_y].GetComponent<SpriteRenderer>().sprite = sprite_block[shape[N_shape].block_id[j]]; break;
                case 79:
                    space_cells_status[cell_x, cell_y] = "2x2 -l-u bombWhite";
                    space_cells_sprite[cell_x, cell_y].GetComponent<SpriteRenderer>().sprite = sprite_block[shape[N_shape].block_id[j]]; break;
                case 80:
                    space_cells_status[cell_x, cell_y] = "2x2 -r-u bombWhite";
                    space_cells_sprite[cell_x, cell_y].GetComponent<SpriteRenderer>().sprite = sprite_block[shape[N_shape].block_id[j]]; break;
                case 81:
                    space_cells_status[cell_x, cell_y] = "2x2 -r-d bombWhite";
                    space_cells_sprite[cell_x, cell_y].GetComponent<SpriteRenderer>().sprite = sprite_block[shape[N_shape].block_id[j]];
                    if (shape[N_shape].drop_shape) 
                    {
                        int randomLucky = UnityEngine.Random.Range(0, 2);  // ����������� ������������ �����
                        if (randomLucky != 0) { DamageCell2x2(cell_x, cell_y, 1); } // ���� ����� ����������, ��� ����������
                    } 
                    break;
                case 82:
                    space_cells_status[cell_x, cell_y] = "2x2 -l-d bombWhite";
                    space_cells_sprite[cell_x, cell_y].GetComponent<SpriteRenderer>().sprite = sprite_block[shape[N_shape].block_id[j] - 4]; break;
                case 83:
                    space_cells_status[cell_x, cell_y] = "2x2 -l-u bombWhite";
                    space_cells_sprite[cell_x, cell_y].GetComponent<SpriteRenderer>().sprite = sprite_block[shape[N_shape].block_id[j] - 4]; break;
                case 84:
                    space_cells_status[cell_x, cell_y] = "2x2 -r-u bombWhite";
                    space_cells_sprite[cell_x, cell_y].GetComponent<SpriteRenderer>().sprite = sprite_block[shape[N_shape].block_id[j] - 4]; break;
                case 85:
                    space_cells_status[cell_x, cell_y] = "2x2 -r-d bombWhite";
                    space_cells_sprite[cell_x, cell_y].GetComponent<SpriteRenderer>().sprite = sprite_block[shape[N_shape].block_id[j] - 4]; break;
                case 86:
                    space_cells_status[cell_x, cell_y] = "2x2 -l-d gorlum crack-00";
                    space_cells_sprite[cell_x, cell_y].GetComponent<SpriteRenderer>().sprite = sprite_block[shape[N_shape].block_id[j]]; break;
                case 87:
                    space_cells_status[cell_x, cell_y] = "2x2 -l-u gorlum crack-00";
                    space_cells_sprite[cell_x, cell_y].GetComponent<SpriteRenderer>().sprite = sprite_block[shape[N_shape].block_id[j]]; break;
                case 88:
                    space_cells_status[cell_x, cell_y] = "2x2 -r-u gorlum crack-00";
                    space_cells_sprite[cell_x, cell_y].GetComponent<SpriteRenderer>().sprite = sprite_block[shape[N_shape].block_id[j]]; break;
                case 89:
                    space_cells_status[cell_x, cell_y] = "2x2 -r-d gorlum crack-00";
                    space_cells_sprite[cell_x, cell_y].GetComponent<SpriteRenderer>().sprite = sprite_block[shape[N_shape].block_id[j]]; break;

            }
        }

        // ���� �������� ������ �� �����, �� ���������
        if (shape[N_shape].drop_shape)
        {
            for (int j = 0; j < shape[N_shape].blocks_count; j++) // ���� �� ������ ������
            {
                int cell_x = (int)(shape[N_shape].x + shape[N_shape].block_pos[j, 0]); //���������� ����� ������. ����� ����� ��� ��������� � ������� ����� ������� space_cells_status
                int cell_y = (int)(shape[N_shape].y + shape[N_shape].block_pos[j, 1]);
                if (cell_y == 1) { break; } // ���� ��� ���� ���
                if (ShapeInCell(cell_x, cell_y - 1) != -1) { continue; } // ���������� ����� ����� ������, ����� ��������� ����� � �����
                if (space_cells_status[cell_x, cell_y - 1].Contains("bomb")) 
                {
                    int randomLucky = UnityEngine.Random.Range(0, 2);  // ����������� ������������ �����
                    if (randomLucky != 0) { DamageCell2x2(cell_x, cell_y - 1, 1); } // ����� �����
                } 
            }
        }
        // ���� ��� �������� ������ ������ (��� ��������� �������), ����������� ���� vip_shape � "-1", ��� ��������, ��� ������ ������� � ������
        if (levelManager.vip_shape == N_shape) { levelManager.vip_shape = -1; }
        CheckSpace(); // �������� ���������� �����
        DeleteShape(); // ������� ������
        levelManager.shapes_count++; // ����������� ������� ���������� �����
        levelManager.SetNewShape(); // ����� ��������� ������ � ���������
        return; // ������ �������������, ��������� ����� ��������� �� �����, �������
    }
    private void DeleteShape() // �������� ������
    {
        for (int i = 0; i < shape[N_shape].blocks_count; i++) // ������� ������� ������� ������
        {
            Destroy(shape[N_shape].block_sprite[i]);
        }
    }

    private void CheckSpace() // �������� ���������� ���� ����� �������
    {
        for (int y_check = 40; y_check > 0; y_check--) // �������� �� ���� ������ ������� ������ ����
        {
            CheckLine(y_check);
        }
    }
    private void CheckLine(int y) // �������� ����� ����� 
    {
        while (true) // ��� ����� ����� ���������� �� ���� ��� �� ���� ������������ ������, �������� �����, ������� � "�������". ����� ��������� ���� ����������
        {
            // ���������, ��������� �� �����
            for (int x = 1; x < 16; x++) // ������� ������ ������
            {
                if (space_cells_status[x, y] == "") { return; } // ������� ���� ������ �� ���������. ����� �� �������, ������� �� �������� ���� �����
            }
            // ����� �������
            sound_crash.Play();
            for (int x = 1; x < 16; x++) // �������� �� ������� ����� ��������� �����
            {
                // ������� ������� ���� �� ������
                if (space_cells_status[x, y].Contains("2x2")) // ��� ���� ����� 2x2
                {
//                    if (space_cells_status[x, y].Contains("bomb")) { DamageCell2x2(x, y); } // ���� ��� �����, �� ����� ����� � ������ ���� ����� ��������, �������� ��� �� �����. �������� ������� ����� ���� ������� � ������� DamageCell2x2
//                    else if (!space_cells_status[x, y].Contains("diamond")) // ����� �� ��������, ����������
//                    {
                        DamageCell2x2(x, y, 2);  // ��������� ����� =2 �� ����� ����� 2x2
//                        if (space_cells_status[x, y] == "") // ���� ����� 2x2 ��������
//                        {
//                            ShiftColumn(x, y_for_shift); // C������� ������� ����� �� ����� ������������� ����� ����� 2x2
//                            ShiftColumn(x + 1, y_for_shift);
//                            ShiftColumn(x, y_for_shift);
//                            ShiftColumn(x + 1, y_for_shift);
//                        }
                        x++; // ���������� ������ ������ ����� 2x2
                }
                else
                {
                    DamageCell(x, y, 1); // ��������� ����� �� ��������� ������
//                    ShiftColumn(x, y); // C������� ������� ����� ������ �� ����� ��������� �����
                }
            }
            /*
             * ��� ���������� ������� ����+������������ ������� ������, ������� ����� �������, ��� ��������������.
             * ���� � ��������� ����� �������� �����, �� � ����� ������� ��� ������ ���� �� ��� ������������ �������.
             * ����� ��������� ���� � ������������, �� ��-�� ������ ����� 2x2 ������� ��������� ����� ��������. ��������, �������� ������(��) �� �������� ���� �����
            */
        }
    }
    private void DamageCell(int x, int y, int power, bool shift = true) // ���� �� ������ �������
    {
        if (space_cells_status[x, y].Contains("2x2")) { DamageCell2x2(x, y, power, shift);  return; } // ��� ������ 2x2 ������� ��������� �������

        switch (space_cells_status[x, y]) // ������� ��� �� ���� � ������
        {
            case "beton": // ������ �����
                if (power == 1)
                {
                    space_cells_status[x, y] = "cracked beton";
                    space_cells_sprite[x, y].GetComponent<SpriteRenderer>().sprite = sprite_block[17]; // ������ ������������� ������
                }
                else // ���� �����������, ����� ��������� ���� �������� ����
                {
                    space_cells_status[x, y] = "";
                    space_cells_sprite[x, y].GetComponent<SpriteRenderer>().sprite = sprite_background; // ������ ������
                }
                break;
            case "diamond": // ����� �� ��������
                FallDiamond(x, y); // �� ����� ������, ���� ������� ����� ��� ��������
                break;
            default: // ����������� ����� �����������
                space_cells_status[x, y] = "";
                space_cells_sprite[x, y].GetComponent<SpriteRenderer>().sprite = sprite_background; // ������ ������
                break;
        }
        if (shift) { ShiftColumn(x, y); } // ���� � ���������� ������, �� �������� ������� �� ����� ������������ ������
    }
    private void ShiftColumn(int target_x, int target_y) // �������� ������� ����� ���� � ��������� ������
    {
        int y = target_y; // ������� ��� ����� �������� ����� �����
        while ( y < 40 ) // ����� ����� �� ��������� ����� "��������" ��� �������� ����� ����
        {
            // �������� ������, ������� ������� ��������
            if (space_cells_status[target_x, y] == "") // �������� � ������ ������. ������� ����������, �� �� ���������� �����. ����� ������ ��� �������� � ������ ����� 2x2
            {
                if (space_cells_status[target_x, y + 1].Contains("2x2")) // ������ ����� ���� ����� 2x2
                {
                    CheckBombTouch(target_x, y + 1); // ���������, ��� �� ��� �����. ���� ����, �� �����
                    space_cells_status[target_x, y] = ""; 
                    space_cells_sprite[target_x, y].GetComponent<SpriteRenderer>().sprite = sprite_background;
                    if (space_cells_status[target_x, y + 1].Contains("diamond")) // ��� ����� 2x2
                    {
                        FallDiamond2x2(target_x, y + 1);
                        return;
                    }
                    else if (space_cells_status[target_x, y + 1].Contains("-r-d"))  //  ������ ������ ���� ����� 2x2, �� ����� ��������� ��� �������� � ������� ��� 2x2 �����, ������ �������
                    {
                        ShiftBlock2x2(target_x, y);
                        return;
                    }
                    // ���� ��� ����� ���� ����� 2x2, �� ���� ������ �� ������. ���� ���� ������������, ����� ����� ������� �� ������� �������
                    return;
                }

                // �������� ������ ������
                if (space_cells_status[target_x, y] != "" && ShapeInCell(target_x, y) != -1) { shape[ShapeInCell(target_x, y)].drop_shape = true; } // ���� ��� �������� ������ ������ ������, �� �����
                space_cells_status[target_x, y] = space_cells_status[target_x, y + 1]; // ������ ������ ������������� �� ������ ������
                space_cells_sprite[target_x, y].GetComponent<SpriteRenderer>().sprite = space_cells_sprite[target_x, y + 1].GetComponent<SpriteRenderer>().sprite; // �� �� �� �������� ������
                space_cells_status[target_x, y + 1] = ""; // ������ ������ ������ ����� ��������
            }
            y++;
        }
        // ������� ����� � ������ ����� ������.
        if (space_cells_status[target_x, target_y] == "diamond") // �� ���������� ������ �������� �������, ������� ���������� ��� ������ ����
        { FallDiamond(target_x, target_y); }
    }
    private void FallDiamond(int x, int y) // ������� �������������� ������� (�����) ����� �������� ����� ��� ���
    {
        while (y > 1) // ���� �� ����� �� ���
        {
            if (space_cells_status[x, y - 1] != "" ) // ���� ����� ������� ������
            {
                // ������������� �����
                CheckSpace(); // ��������� ������������� ���������, �� ��������� �� ��� ���� �����. ����� ���� �� ������ ���� �����, �� ��� ��������� ���� �������� ���
                return; // � �������
            }
            // ���� ����� ��������� ������, ������� �� � ����� ����� � ������� ����� �� ��
            if (ShapeInCell(x, y - 1) != -1) { shape[ShapeInCell(x, y - 1)].drop_shape = true; } // ���� ��� �������� ������ ������ ������, �� �����
            ShiftColumn(x, y - 1);
            y--;
        }
        // ����� �� ��� �������. ���������� ������ ������
        // ����� ������� �� ����� 
        space_cells_status[x, 1] = "";
        ShiftColumn(x, 1); // �������� ������� ����� �� ����� ��������� ������
    }




    // ����� 2x2
    private void DamageCell2x2(int x, int y, int power, bool shift = true) // ���� �� ����� ����� 2x2 �������, ���� �������� ��� ������ ������
    {
        if (!space_cells_status[x, y].Contains("2x2")) { Debug.Log("ALARM! � DamageCell2x2 ��������� ���� �� " + space_cells_status[x, y] + " (" + x + ":" + y + ")"); Debug.Break();  return; } // ������ 2x2 ��� ���������
        // ����������� ������� ������� ����
        switch (space_cells_status[x, y].Substring(4, 4)) // ������� ������ ������ ����
        {
            case "-l-u":
                x = x + 1;
                y = y - 1;
                break;
            case "-r-u":
                y = y - 1;
                break;
            case "-l-d":
                x = x + 1;
                break;
        }
        if (space_cells_status[x, y].Contains("diamond")) { FallDiamond2x2(x, y); return; } // ����� �� ��������, �� ����� ���������� ���� ���� ��� ��� ����� �����
        else if (space_cells_status[x, y].Contains("bombWhite")) { Flash2x2(x, y); return; } // ����� ����� ����� �������
        else if (space_cells_status[x, y].Contains("bombBlack")) { Blast2x2(x, y); return; } // ����� ������ ����� c ����� � �������
        else if (space_cells_status[x, y].Contains("beton"))
        {
            // �������� 2x2 � 16-��� �������
            int startIndex = space_cells_status[x, y].LastIndexOf('-') + 1; // ��������� ������� "-" � ������� ������
            string numberString = space_cells_status[x, y].Substring(startIndex); // ������� �������� ������ (������� ��� ���� �����������) � ��������� �������
            int count_damage = int.Parse(numberString); // ����������� � �����
                                                        // ����������� ����� ����������� ����� ������ �� 1
            count_damage = count_damage + power;
            // �������� ����� � ������� �� ����� ��������
            if (count_damage < 16)
            {
                space_cells_status[x - 1, y] = space_cells_status[x - 1, y].Substring(0, startIndex) + count_damage.ToString();
                space_cells_sprite[x - 1, y].GetComponent<SpriteRenderer>().sprite = sprite_block[34 + Mathf.FloorToInt(count_damage / 4) * 4]; // ������ ������������� ������ ������� ���� ����� 2�2
                space_cells_status[x - 1, y + 1] = space_cells_status[x - 1, y + 1].Substring(0, startIndex) + count_damage.ToString();
                space_cells_sprite[x - 1, y + 1].GetComponent<SpriteRenderer>().sprite = sprite_block[35 + Mathf.FloorToInt(count_damage / 4) * 4]; // ������ ������������� ������ �������� ���� ����� 2�2
                space_cells_status[x, y + 1] = space_cells_status[x, y + 1].Substring(0, startIndex) + count_damage.ToString();
                space_cells_sprite[x, y + 1].GetComponent<SpriteRenderer>().sprite = sprite_block[36 + Mathf.FloorToInt(count_damage / 4) * 4]; // ������ ������������� ������� �������� ���� ����� 2�2
                space_cells_status[x, y] = space_cells_status[x, y].Substring(0, startIndex) + count_damage.ToString();
                space_cells_sprite[x, y].GetComponent<SpriteRenderer>().sprite = sprite_block[37 + Mathf.FloorToInt(count_damage / 4) * 4]; // ������ ������������� ������� ������� ���� ����� 2�2
            }
            else { clearCell2x2(x, y, shift); } // ���� ��������
        }
        else if (space_cells_status[x, y].Contains("gorlum"))
        {
            // ������
            int startIndex = space_cells_status[x, y].LastIndexOf('-') + 1; // ��������� ������� "-" � ������� ������
            string numberString = space_cells_status[x, y].Substring(startIndex); // ������� �������� ������ (������� ��� ���� �����������) � ��������� �������
            int count_damage = int.Parse(numberString); // ����������� � �����
                                                        // ����������� ����� ����������� ����� ������ �� 1
            count_damage++;
            // �������� ����� � ������� �� ����� ��������
            if (count_damage < 40)
            {
                space_cells_status[x - 1, y] = space_cells_status[x - 1, y].Substring(0, startIndex) + count_damage.ToString();
                space_cells_status[x - 1, y + 1] = space_cells_status[x - 1, y + 1].Substring(0, startIndex) + count_damage.ToString();
                space_cells_status[x, y + 1] = space_cells_status[x, y + 1].Substring(0, startIndex) + count_damage.ToString();
                space_cells_status[x, y] = space_cells_status[x, y].Substring(0, startIndex) + count_damage.ToString();
            }
            else { clearCell2x2(x, y, shift); } // ���� ��������
        }
    }
    private void clearCell2x2(int x, int y, bool shift)
    {
        space_cells_status[x - 1, y + 1] = "";
        space_cells_sprite[x - 1, y + 1].GetComponent<SpriteRenderer>().sprite = sprite_background;
        space_cells_status[x, y + 1] = "";
        space_cells_sprite[x, y + 1].GetComponent<SpriteRenderer>().sprite = sprite_background;
        space_cells_status[x - 1, y] = "";
        space_cells_sprite[x - 1, y].GetComponent<SpriteRenderer>().sprite = sprite_background;
        space_cells_status[x, y] = "";
        space_cells_sprite[x, y].GetComponent<SpriteRenderer>().sprite = sprite_background;
        if (shift) 
        {
            ShiftColumn(x - 1, y + 1);
            ShiftColumn(x, y + 1);
            ShiftColumn(x - 1, y);
            ShiftColumn(x, y);
        }
    }


    private void ShiftBlock2x2(int free_x, int free_y) // �������� ����� 2x2 � ��������� ������ (������ �����)
    {
        if (space_cells_status[free_x - 1, free_y] != "" ||
            space_cells_status[free_x, free_y] != "") { return; } // ��� ����� ����� 2x2 ������ ���� ��������
        // ��������� ������ ����� 2x2 �� ���� ������ ����
        if (ShapeInCell(free_x, free_y - 1) != -1) { shape[ShapeInCell(free_x, free_y - 1)].drop_shape = true; } // ���� ��� �������� ����� ������ ������ ������, �� �����
        if (ShapeInCell(free_x - 1, free_y - 1) != -1) { shape[ShapeInCell(free_x - 1, free_y - 1)].drop_shape = true; } // ���� ��� �������� ������ ������ ������ ������, �� �����
        space_cells_status[free_x - 1, free_y] = space_cells_status[free_x - 1, free_y + 1];
        space_cells_status[free_x, free_y] = space_cells_status[free_x, free_y + 1];
        space_cells_status[free_x - 1, free_y + 1] = space_cells_status[free_x - 1, free_y + 2];
        space_cells_status[free_x, free_y + 1] = space_cells_status[free_x, free_y + 2];
        space_cells_status[free_x - 1, free_y + 2] = "";
        space_cells_status[free_x, free_y + 2] = "";
        // � �������
        space_cells_sprite[free_x - 1, free_y].GetComponent<SpriteRenderer>().sprite = space_cells_sprite[free_x - 1, free_y + 1].GetComponent<SpriteRenderer>().sprite;
        space_cells_sprite[free_x, free_y].GetComponent<SpriteRenderer>().sprite = space_cells_sprite[free_x, free_y + 1].GetComponent<SpriteRenderer>().sprite;
        space_cells_sprite[free_x - 1, free_y + 1].GetComponent<SpriteRenderer>().sprite = space_cells_sprite[free_x - 1, free_y + 2].GetComponent<SpriteRenderer>().sprite;
        space_cells_sprite[free_x, free_y + 1].GetComponent<SpriteRenderer>().sprite = space_cells_sprite[free_x, free_y + 2].GetComponent<SpriteRenderer>().sprite;

        ShiftColumn(free_x - 1, free_y + 2); // �������� ����� ������� ����� ��� ������ 2x2
        ShiftColumn(free_x, free_y + 2); // �������� ������ ������� ����� ��� ������ 2x2
    }
    private void FallDiamond2x2(int x, int y) // ������� ������ 2x2 ����� �������� ����� ��� ���. ��������� - ���������� ������� ������� ����
    {
        // ������������� ����
        if (!space_cells_status[x, y].Contains("-r-d")) { x = x + 1; } // ������ ������� ��������� �� ����� ����
        while (y > 1) // ���� �� ����� �� ���
        {
            if (space_cells_status[x, y - 1] != "" ||
                space_cells_status[x - 1, y - 1] != "")
            {
                // ������������� �����
                CheckSpace(); // ��������� ��������� �����
                return; // � �������
            }
            // � ���� ����� ��� ������ ��������, �������� ���� ���� �����
            // ��������� ������ ����� 2x2 �� ���� ������ ����
            ShiftBlock2x2(x, y - 1);
            y--;
        }
        // ����� �� ��� �������. ���������� ������ ������
        // ����� ������� �� ����� 
        space_cells_status[x, y] = "";
        space_cells_status[x - 1, y] = "";
        space_cells_status[x, y + 1] = "";
        space_cells_status[x - 1, y + 1] = "";
        ShiftBlock2x2(x, y); // ��� ��� �� �������������
        ShiftBlock2x2(x + 1, y);
        ShiftBlock2x2(x, y);
        ShiftBlock2x2(x + 1, y + 1);
    }




    // �����
    private void CheckBombTouch(int x, int y) // ��������, �� ������� �� ����� ��� ������? ��� ����� ������� ����� �������� �����, ��������� ��������� ������ � ����� ���������� :)
    {
        if (space_cells_status[x, y].Contains("bomb")) { DamageCell2x2(x, y, 1); }
    }
    private void Flash2x2(int x, int y) // ������� ����� 2x2 ��� ���� � �������
    {
        Instantiate(prefabFlash, new Vector3(x - 0.5f, y + 0.5f, 0), Quaternion.identity); // �������� �������

        // ������� �������� � ����� ������, ����� �� ������� ����������� ���� ��-�� ��������� �������� ����
        space_cells_status[x - 1, y] = "";
        space_cells_sprite[x - 1, y].GetComponent<SpriteRenderer>().sprite = sprite_background; // ������ ������
        space_cells_status[x, y] = "";
        space_cells_sprite[x, y].GetComponent<SpriteRenderer>().sprite = sprite_background;
        space_cells_status[x - 1, y + 1] = "";
        space_cells_sprite[x - 1, y + 1].GetComponent<SpriteRenderer>().sprite = sprite_background;
        space_cells_status[x, y + 1] = "";
        space_cells_sprite[x, y + 1].GetComponent<SpriteRenderer>().sprite = sprite_background;
        // ��������� �����
        int power = 3; // ���� ������
        // ����� ������ - ���� � ��������, ������ ���� ������. �� ������ ����� ����� 2x2, x:y - ������� ������ ������.
        float x_center = x - 0.5f; float y_center = y + 0.5f; // �������� ��������� ������ ������ �� ������� ������� ���� �����
        for (float burnedCell_x = x_center - power - 0.5f; burnedCell_x <= x_center + power + 0.5f; burnedCell_x++) // ������� ���� �� �����������
        {
            if (burnedCell_x < 1 || burnedCell_x > (16 - 1)) { continue; } // ���� �� ����������� ����� �� �����, �� ����������
            // ������ ����� �� ���� �������� �� ������ ������. �������� � ������
            float x_power = power - Mathf.Abs(x_center - burnedCell_x + 0.5f * (burnedCell_x - x_center) / Mathf.Abs(burnedCell_x - x_center)); // ������ ����� �� ���� �������� �� ������ ������. �������� � ������

            for (float burnedCell_y = y_center + x_power + 0.5f; burnedCell_y >= y_center - x_power - 0.5f; burnedCell_y--) // ������� ���� �� ���������. �������� ������ ����, ����� �� ������������ ������, ������������ �� ����� ��������� 2x2. 
            {
                if (burnedCell_y < 1 || burnedCell_y > 40) { continue; } // ���� �� ��������� ����� �� �����, �� ����������
                int damage_power = (int)(x_power - Mathf.Abs(y_center - burnedCell_y) + 0.5f + 1f); // ���������� ����� ������� �� ���������� ������ �� ������ ������
                if (space_cells_status[(int)burnedCell_x, (int)burnedCell_y] != "")
                {
                    DamageCell((int)burnedCell_x, (int)burnedCell_y, damage_power);   // ��������� ����� �� ������. ���������� ��� ������ ����
                }
            }
        }
    }
    private void Blast2x2(int x, int y) // ����� ����� 2x2 � �����
    {
        // ������� �������� � ����� ������, ����� �� ������� ����������� ���� ��-�� ��������� �������� ����
        space_cells_status[x - 1, y] = "";
        space_cells_sprite[x - 1, y].GetComponent<SpriteRenderer>().sprite = sprite_background; // ������ ������
        space_cells_status[x, y] = "";
        space_cells_sprite[x, y].GetComponent<SpriteRenderer>().sprite = sprite_background;
        space_cells_status[x - 1, y + 1] = "";
        space_cells_sprite[x - 1, y + 1].GetComponent<SpriteRenderer>().sprite = sprite_background;
        space_cells_status[x, y + 1] = "";
        space_cells_sprite[x, y + 1].GetComponent<SpriteRenderer>().sprite = sprite_background;
        // ��������� �����
        int power = 4; // ���� ������
        // ����� ������ - ���� � ��������, ������ ���� ������. �� ������ ����� ����� 2x2, x:y - ������� ������ ������.
        float x_center = x - 0.5f; float y_center = y + 0.5f; // �������� ��������� ������ ������ �� ������� ������� ���� �����
        for (float burnedCell_x = x_center - power - 0.5f; burnedCell_x <= x_center + power + 0.5f; burnedCell_x++) // ������� ���� �� �����������
        {
            if (burnedCell_x < 1 || burnedCell_x > (16 - 1)) { continue; } // ���� �� ����������� ����� �� �����, �� ����������
            // ������ ����� �� ���� �������� �� ������ ������. �������� � ������
            float x_power = power - Mathf.Abs(x_center - burnedCell_x + 0.5f * (burnedCell_x - x_center) / Mathf.Abs(burnedCell_x - x_center)); // ������ ����� �� ���� �������� �� ������ ������. �������� � ������

            for (float burnedCell_y = y_center + x_power + 0.5f; burnedCell_y >= y_center - x_power - 0.5f; burnedCell_y--) // ������� ���� �� ���������. �������� ������ ����, ����� �� ������������ ������, ������������ �� ����� ��������� 2x2. 
            {
                if (burnedCell_y < 1 || burnedCell_y > 40) { continue; } // ���� �� ��������� ����� �� �����, �� ����������
                int damage_power = (int)(x_power - Mathf.Abs(y_center - burnedCell_y) + 0.5f + 1f); // ���������� ����� ������� �� ���������� ������ �� ������ ������
                if (space_cells_status[(int)burnedCell_x, (int)burnedCell_y] != "") 
                {
                    DamageCell((int)burnedCell_x, (int)burnedCell_y, damage_power, true);   // ��������� ����� �� ������. ���������� �� ������� ����
                }
                ShiftColumn((int)burnedCell_x, (int)burnedCell_y); // ����� ����� ��-�� ������
                GameObject smoke = Instantiate(prefabSmoke, new Vector3(burnedCell_x, burnedCell_y, 0), Quaternion.identity); // ������� ��������� ������� ����
                SpriteRenderer spriteRenderer = smoke.GetComponent<SpriteRenderer>(); // �������� ��� SpriteRenderer
                spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, (float)damage_power / (float)(power - 0.001f) * 1.5f); // ��������� ������� ���� � ����������� �� �������� �� ������
            }
        }
        sound_boom.Play(); // ���� ������
        CheckSpace(); // �������� �����, ����������� ��-�� ���������
    }




    // �������� �����

    private void SetShapeDefaultValues() // �������� ��-��������� ��� ������������ ������
    {
        // ���������� ������ ������ ��-���������
        if (N_shape == 0) { shape[N_shape].x = 3f; }
        else { shape[N_shape].x = 12f; }
        shape[N_shape].y = 19f;

        shape[N_shape].block_id = new int[shape[N_shape].blocks_count]; // ������ ������ id �������� ������ ������
        shape[N_shape].block_sprite = new GameObject[shape[N_shape].blocks_count];  // ������ ������ ����� �������� (��������)
        shape[N_shape].speed = 1f;
        shape[N_shape].can_rotate = true;
        shape[N_shape].can_rotate_sprite = false;
}

    public void MakeNewClassicShape() // �������� ����� ������ ������������� �������. �������� count_beton - ����, ����������� �������� �����, ������� ����������� � ���� ������
    {
        shape[N_shape] = new TetrixShape();
        shape[N_shape].blocks_count = 4;
        SetShapeDefaultValues();
        int randomShape = UnityEngine.Random.Range(0, 7);  // �������� �������� ���� �� 7 ��������� ����������� �������
        //randomShape = 5;
        switch (randomShape)
        {
            case 0: // "O"
                // ������ ������� ������, ������������� ������
                shape[N_shape].block_pos = new float[,] { { -0.5f, -0.5f }, { -0.5f, 0.5f }, { 0.5f, 0.5f }, { 0.5f, -0.5f } };
                shape[N_shape].x = shape[N_shape].x + 1.5f; shape[N_shape].y = 18.5f; // � ������ ��� ����� � ��������
                break;
            case 1:  // "S"
                shape[N_shape].block_pos = new float[,] { { -1f, 0f }, { 0f, 0f }, { 0f, 1f }, { 1f, 1f } };
                break;
            case 2:  // "Z"
                shape[N_shape].block_pos = new float[,] { { -1f, 1f }, { 0f, 1f }, { 0f, 0f }, { 1f, 0f } };
                break;
            case 3:  // "I"
                shape[N_shape].block_pos = new float[,] { { -2f, 0f }, { -1f, 0f }, { 0f, 0f }, { 1f, 0f } };
                break;
            case 4:  // "L"
                shape[N_shape].block_pos = new float[,] { { 0f, 1f }, { 0f, 0f }, { 0f, -1f }, { 1f, -1f } };
                break;
            case 5:  // "J"
                shape[N_shape].block_pos = new float[,] { { 0f, 1f }, { 0f, 0f }, { 0f, -1f }, { -1f, -1f } };
                break;
            case 6:  // "T"
                shape[N_shape].block_pos = new float[,] { { -1f, 0f }, { 0f, 0f }, { 0f, 1f }, { 1f, 0f } };
                break;
        }
        for (int i = 0; i < shape[N_shape].blocks_count; i++) // ���� ������ ����������� ���������� id ��������
        {
            shape[N_shape].block_id[i] =  randomShape + 1;
        }
    }
    public void MakeBetonBlock(int count_beton)
    {
        /* ����
        count_beton - ���������� �������� ������,
        ������ � ���������� ����� id �� �������� ������ �� �����.
        ��������� ��� count_beton ���. 
        */
        MakeNewClassicShape();
        for (int i = 0; i < count_beton; i++)
        {
            while (true)
            {
                int randomId = UnityEngine.Random.Range(0, 4);  //  ��������� ����� �� 0 �� 3 (������������)
                if (shape[N_shape].block_id[randomId] < 9) // �������� ���� ������ ������ ������ ��������, ������ ������ ���� �� ������� �������
                {
                    shape[N_shape].block_id[randomId] = shape[N_shape].block_id[randomId] + 9; // id ��������� �����
                    break;
                }
            }
        }
    }
    public void MakeClassicShape2x2(int mode) // �������� ������� ������ ������ �� ������ 2x2
    {
        shape[N_shape].blocks_count = 16;
        SetShapeDefaultValues();
        int ld_id = 0; // id ������� ������ ������� ����. ��� ������ ������ �����
        switch (mode)
        {
            case 0:  // "O"
                shape[N_shape].block_pos = new float[,] { { -1.5f, -1.5f }, { -1.5f, -0.5f }, { -0.5f, -0.5f }, { -0.5f, -1.5f },
                    { -1.5f, 0.5f }, { -1.5f, 1.5f }, { -0.5f, 1.5f }, { -0.5f, 0.5f },
                    { 0.5f, 0.5f }, { 0.5f, 1.5f }, { 1.5f, 1.5f }, { 1.5f, 0.5f },
                    { 0.5f, -1.5f }, { 0.5f, -0.5f }, { 1.5f, -0.5f }, { 1.5f, -1.5f }}; // ������ ������� ������, ������������� ������
                shape[N_shape].can_rotate = false;
                ld_id = 50;  break;
            case 1:  // "S"
                shape[N_shape].block_pos = new float[,] { { -2.5f, -1.5f }, { -2.5f, -0.5f }, { -1.5f, -0.5f }, { -1.5f, -1.5f },
                    { -0.5f, -1.5f }, { -0.5f, -0.5f }, { 0.5f, -0.5f }, { 0.5f, -1.5f },
                    { -0.5f, 0.5f }, { -0.5f, 1.5f }, { 0.5f, 1.5f }, { 0.5f, 0.5f },
                    { 1.5f, 0.5f }, { 1.5f, 1.5f }, { 2.5f, 1.5f }, { 2.5f, 0.5f }}; // ������ ������� ������, ������������� ������
                ld_id = 54; break;
            case 2:  // "Z"
                shape[N_shape].block_pos = new float[,] { { -2.5f, 0.5f }, { -2.5f, 1.5f }, { -1.5f, 1.5f }, { -1.5f, 0.5f },
                    { -0.5f, -1.5f }, { -0.5f, -0.5f }, { 0.5f, -0.5f }, { 0.5f, -1.5f },
                    { -0.5f, 0.5f }, { -0.5f, 1.5f }, { 0.5f, 1.5f }, { 0.5f, 0.5f },
                    { 1.5f, -1.5f }, { 1.5f, -0.5f }, { 2.5f, -0.5f }, { 2.5f, -1.5f }}; // ������ ������� ������, ������������� ������
                ld_id = 58; break;
            case 3:  // "I"
                shape[N_shape].block_pos = new float[,] { { -0.5f, -3.5f }, { -0.5f, -2.5f }, { 0.5f, -2.5f }, { 0.5f, -3.5f },
                    { -0.5f, -1.5f }, { -0.5f, -0.5f }, { 0.5f, -0.5f }, { 0.5f, -1.5f },
                    { -0.5f, 0.5f }, { -0.5f, 1.5f }, { 0.5f, 1.5f }, { 0.5f, 0.5f },
                    { -0.5f, 2.5f }, { -0.5f, 3.5f }, { 0.5f, 3.5f }, { 0.5f, 2.5f }}; // ������ ������� ������, ������������� ������
                ld_id = 62; break;
            case 4:  // "L"
                shape[N_shape].block_pos = new float[,] { { -1.5f, -2.5f }, { -1.5f, -1.5f }, { -0.5f, -1.5f }, { -0.5f, -2.5f },
                    { 0.5f, -2.5f }, { 0.5f, -1.5f }, { 1.5f, -1.5f }, { 1.5f, -2.5f },
                    { -1.5f, -0.5f }, { -1.5f, 0.5f }, { -0.5f, 0.5f }, { -0.5f, -0.5f },
                    { -1.5f, 1.5f }, { -1.5f, 2.5f }, { -0.5f, 2.5f }, { -0.5f, 1.5f }}; // ������ ������� ������, ������������� ������
                ld_id = 66; break;
            case 5:  // "J"
                shape[N_shape].block_pos = new float[,] { { -1.5f, -2.5f }, { -1.5f, -1.5f }, { -0.5f, -1.5f }, { -0.5f, -2.5f },
                    { 0.5f, -2.5f }, { 0.5f, -1.5f }, { 1.5f, -1.5f }, { 1.5f, -2.5f },
                    { 0.5f, -0.5f }, { 0.5f, 0.5f }, { 1.5f, 0.5f }, { 1.5f, -0.5f },
                    { 0.5f, 1.5f }, { 0.5f, 2.5f }, { 1.5f, 2.5f }, { 1.5f, 1.5f }}; // ������ ������� ������, ������������� ������
                ld_id = 70; break;
            case 6:  // "T"
                shape[N_shape].block_pos = new float[,] { { -2.5f, -1.5f }, { -2.5f, -0.5f }, { -1.5f, -0.5f }, { -1.5f, -1.5f },
                    { -0.5f, -1.5f }, { -0.5f, -0.5f }, { 0.5f, -0.5f }, { 0.5f, -1.5f },
                    { -0.5f, 0.5f }, { -0.5f, 1.5f }, { 0.5f, 1.5f }, { 0.5f, 0.5f },
                    { 1.5f, -1.5f }, { 1.5f, -0.5f }, { 2.5f, -0.5f }, { 2.5f, -1.5f }}; // ������ ������� ������, ������������� ������
                ld_id = 74; break;
        }

        if (N_shape == 0) { shape[N_shape].x = 4.5f; }
        else { shape[N_shape].x = 10.5f; }
        shape[N_shape].y = 25.5f;


        for (int i = 0; i < shape[N_shape].blocks_count; i = i + 4)
        {
            shape[N_shape].block_id[i] = ld_id;
            shape[N_shape].block_id[i + 1] = ld_id + 1;
            shape[N_shape].block_id[i + 2] = ld_id + 2;
            shape[N_shape].block_id[i + 3] = ld_id + 3;
        }
    }
    public void MakeDiamond() // �������� ������
    {
        shape[N_shape].blocks_count = 1;
        SetShapeDefaultValues();
        shape[N_shape].block_pos = new float[,] { { 0f, 0f } }; // ������ �� ������������ ������� �����, ������������ ������
        shape[N_shape].can_rotate = false;
        shape[N_shape].block_id[0] = 8; // �������� �������
    }
    public void MakeDiamond2x2() // �������� ������ 2x2
    {
        shape[N_shape].blocks_count = 24;
        SetShapeDefaultValues();
        shape[N_shape].x = 10.5f; shape[N_shape].y = 30.5f;
        shape[N_shape].block_pos = new float[,] { { -1.5f, -1.5f }, { -1.5f, -0.5f }, { -0.5f, -0.5f }, { -0.5f, -1.5f },
                    { -1.5f, 0.5f }, { -1.5f, 1.5f }, { -0.5f, 1.5f }, { -0.5f, 0.5f },
                    { 0.5f, 0.5f }, { 0.5f, 1.5f }, { 1.5f, 1.5f }, { 1.5f, 0.5f },
                    { 0.5f, -1.5f }, { 0.5f, -0.5f }, { 1.5f, -0.5f }, { 1.5f, -1.5f },
                    { -0.5f, 2.5f }, { -0.5f, 3.5f }, { 0.5f, 3.5f }, { 0.5f, 2.5f },
                    { -0.5f, 4.5f }, { -0.5f, 5.5f }, { 0.5f, 5.5f }, { 0.5f, 4.5f },}; // ������ ������� ������, ������������� ������
        shape[N_shape].can_rotate = false;
        for (int i = 0; i < 16; i = i + 4)
        {
            shape[N_shape].block_id[i] = 50;
            shape[N_shape].block_id[i + 1] = 51;
            shape[N_shape].block_id[i + 2] = 52;
            shape[N_shape].block_id[i + 3] = 53;
        }
        for (int i = 16; i < 20; i++)
        {
            shape[N_shape].block_id[i] = 86 + i - 16; // ������� �� �������� �������
        }
        for (int i = 20; i < shape[N_shape].blocks_count; i++)
        {
            shape[N_shape].block_id[i] = 26 + i - 20; // ������� �� �������� ������
        }
    }
    public void MakeBoxBombBlack() // �������� ������� � ������ ������
    {
        shape[N_shape].blocks_count = 4;
        SetShapeDefaultValues();
        shape[N_shape].block_pos = new float[,] { { -1f, 0f }, { -1f, 1f }, { 0f, 1f }, { 0f, 0f }}; // ������ ������� ������, ������������� ������
        shape[N_shape].can_rotate = false;
        for (int i = 0; i < shape[N_shape].blocks_count; i++)
        {
            shape[N_shape].block_id[i] = 22 + i; // ������� �� �������� �����
        }
    }
    public void MakeBombBlack() // �������� ������ ����� ��� �������
    {
        shape[N_shape].blocks_count = 4;
        SetShapeDefaultValues();
        shape[N_shape].block_pos = new float[,] { { -1f, 0f }, { -1f, 1f }, { 0f, 1f }, { 0f, 0f } }; // ������ ������� ������, ������������� ������
        shape[N_shape].can_rotate = false;
        for (int i = 0; i < shape[N_shape].blocks_count; i++)
        {
            shape[N_shape].block_id[i] = 18 + i; // ������� �� �������� ������ �����
        }
    }
    public void MakeBoxBombWhite() // �������� ������� � ����� ������
    {
        shape[N_shape].blocks_count = 4;
        SetShapeDefaultValues();
        shape[N_shape].block_pos = new float[,] { { -1f, 0f }, { -1f, 1f }, { 0f, 1f }, { 0f, 0f } }; // ������ ������� ������, ������������� ������
        shape[N_shape].can_rotate = false;
        for (int i = 0; i < shape[N_shape].blocks_count; i++)
        {
            shape[N_shape].block_id[i] = 82 + i; // ������� �� �������� �����
        }
    }
    public void MakeBombWhite() // �������� ����� ����� ��� �������
    {
        shape[N_shape].blocks_count = 4;
        SetShapeDefaultValues();
        shape[N_shape].block_pos = new float[,] { { -1f, 0f }, { -1f, 1f }, { 0f, 1f }, { 0f, 0f } }; // ������ ������� ������, ������������� ������
        shape[N_shape].can_rotate = false;
        for (int i = 0; i < shape[N_shape].blocks_count; i++)
        {
            shape[N_shape].block_id[i] = 78 + i; // ������� �� �������� ����� �����
        }
    }
}
