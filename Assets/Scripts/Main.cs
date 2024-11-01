using UnityEngine;
using System.Collections;

public struct TetrixShape // Структура для значений опускающейся фигуры
{
    public float x, y; // Координаты центра
    public int blocks_count; // Количество блоков
    public float[,] block_pos; // Массив смещений блоков относительно центра. [dX, dY]
    public GameObject[] block_sprite; // Массив объектов - спрайтов для блоков
    public int[] block_id; // Массив идентификаторов спрайтов блоков из общеигрового массива спрайтов sprite_block
    public float nextTime; // Время следующего спуска фигуры
    public bool can_rotate; // Можно ли вращать фигуру
    public bool can_rotate_sprite; // Можно ли вращать спрайты блоков
    public float speed; // Скорость падения фигуры (секунд на линию)
    public bool drop_shape; // Включен сброс фигуры
}
public class Main : MonoBehaviour
{
    public LevelManager levelManager; // ссылка на скрипт, управления уровнями игры

    public static bool stop_game = true; // Остановить ли движение фигур? Пауза и стоп-игра

    //private int y_check; // Номер проверяемой линии.
    private GameObject[,] space_cells_sprite; // Двумерный массив объектов-спрайтов стакана
    public string[,] space_cells_status; // Двумерный массив статусов ячеек стакана (текстовое описание для удобства)
    [SerializeField] Sprite sprite_background; // Спрайт пустой ячейки стакана
    [SerializeField] GameObject prefabSmoke; // Префаб дыма
    [SerializeField] GameObject prefabFlash; // Префаб вспышки взрыва

    private TetrixShape[] shape = new TetrixShape[2]; // Массив фигур
    public int N_shape; // Номер фигуры, обрабатываевой в данный момент
    public Sprite[] sprite_block; // Общий массив спрайтов блоков (все картинки). Наполняется в Инспекторе

    // Звуки
    [SerializeField] AudioSource sound_pong; // Фиксация фигуры
    [SerializeField] AudioSource sound_crash; // Удаление линии
    [SerializeField] AudioSource sound_boom; // Взрыв чёрной бомбы

    private void Awake()
    {
        stop_game = true; // Инициализирум статическую переменную
    }
    public void PrepareGame() // Подготовка игры
    {
        Debug.ClearDeveloperConsole(); // Очистка консоли
        UnityEngine.Random.InitState(System.DateTime.Now.Millisecond); // Инициализация псевдослучайного генератора чтобы не выпадали одни и те же фигуры

        // Заполняем стакан Объектами, которые в ходе игры будут менять свой спрайт, как светофор. Храняться в двумерном массиве space_cells_sprite[,]
        // Одновременно заполняем аналогичный массив space_cells_status[,] с текстовым описанием блока, находящегося в этой ячейке
        space_cells_sprite = new GameObject[16, 41]; // Двумерный массив объектов-спрайтов блоков фигуры
        space_cells_status = new string[16, 41]; // Двумерный массив статусов ячеек стакана
        for (int j = 1; j <= 40; j++) // по Y
        {
            for (int i = 1; i < 16; i++) // по X
            {
                space_cells_sprite[i, j] = new GameObject("Cell(" + i + "," + j + ")");
                // Добавляем компонент SpriteRenderer к ячейке
                SpriteRenderer spriteRenderer = space_cells_sprite[i, j].AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = sprite_background;
                // Позиционируем ячейку
                space_cells_sprite[i, j].transform.position = new Vector3(i * 1f, j * 1f, 0);
                spriteRenderer.sortingOrder = 1; // Позиция слоя - перед стаканом, но за фигурами
                // Изначально ячейка пуста
                space_cells_status[i, j] = "";
            }
        }
        stop_game = false; // Снимаем с паузы
    }


    public void RenderNewShape() // Отрисовать новую фигуру на сцене
    {
        int block_x; // Позиция блока фигуры в стакане. Нужны целые для обращения к массиву координат ячеек стакана
        int block_y;

        // Проверка, есть ли на месте спауна другая фигура
        // При наложении будем поднимать новую фигуру вверх
        bool have_place = false; // Определено ли место для новой фигуры
        float try_y = shape[N_shape].y; // Предполагаемая высота новой фигуры
        shape[N_shape].y = 100f; // Пока поднимем фигуру выше, чтобы точно не совпадала с мешающей фигурой
        while (!have_place) // Будем поднимать предполагаемое место спауна пока все блоки не покинут мешающую фигуры
        {
            have_place = true; // Предположим, что место свободно
            for (int i = 0; i < shape[N_shape].blocks_count; i++) // Цикл по блокам фигуры
            {
                block_x = (int)(shape[N_shape].x + shape[N_shape].block_pos[i, 0]); // Позиция блока фигуры
                block_y = (int)(try_y + shape[N_shape].block_pos[i, 1]);
                have_place = (ShapeInCell(block_x, block_y) == -1 || ShapeInCell(block_x, block_y) == N_shape); // Не занято ли место под новый блок другой фигурой?
                if (!have_place) // Если место под блоком занято другой фигурой
                { try_y++; break; } // Поднимаем всю фигуру вверх на одну линию и выходим из цикла для повторной проверки
            }
        }
        shape[N_shape].y = try_y; // С высотой для новой фигурой определились

        // Генерация объектов-спрайтов для визуализации фигуры
        for (int i = 0; i < shape[N_shape].blocks_count; i++) // Цикл по блокам фигуры
        {
            // Проверка, а не проиграл ли ты?
            block_x = (int)(shape[N_shape].x + shape[N_shape].block_pos[i, 0]); // Координаты блока фигуры. Нужны целые для обращения к массиву координат блоков
            block_y = (int)(shape[N_shape].y + shape[N_shape].block_pos[i, 1]);
            if (space_cells_status[block_x, block_y] != "") { levelManager.GameOver(); } // Проиграл, ячейка стакана уже не пуста

            shape[N_shape].block_sprite[i] = new GameObject("Shape_Cell(" + i + ")"); // Создаём блоки фигуры (объекты-спрайты)
            //Добавляем компонент SpriteRenderer к блоку
            SpriteRenderer spriteRenderer = shape[N_shape].block_sprite[i].AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite_block[shape[N_shape].block_id[i]]; // Выбираем спрайт блока из массива
            spriteRenderer.sortingOrder = 2; // Позиция слоя - перед ячейками стакана
        }
        // Позиционируем блоки (переносим на места в соответствии с массивом смещений shape[N_shape].block_pos[] + центр фигуры)
        for (int i = 0; i < shape[N_shape].blocks_count; i++)
        {
            shape[N_shape].block_sprite[i].transform.position = new Vector3(shape[N_shape].x + shape[N_shape].block_pos[i, 0], shape[N_shape].y + shape[N_shape].block_pos[i, 1], 0);
        }
        shape[N_shape].drop_shape = false;
        shape[N_shape].nextTime = Time.time + shape[N_shape].speed; // Время следующего опускания фигуры наступит через секунду
        N_shape = -1; // работа с фигурой окончена, отпускаем её
    }
    private int ShapeInCell(int x, int y) // Определение id фигуры, находящийся в ячейке. "-1" - если в ячейке нет фигур
    {
        for (int check_shape = 0; check_shape <= 1; check_shape++) // Рассматриваем обе фигуры по очереди
        {
            for (int i = 0; i < shape[check_shape].blocks_count; i++) // Перебираем все блоки рассматриваемой фигуры
            {
                if (shape[check_shape].x + shape[check_shape].block_pos[i, 0] == x &&
                    shape[check_shape].y + shape[check_shape].block_pos[i, 1] == y) // Если позиция ячейки-аргумента совпадают с позицией блока фигуры, то возвращаем id это фигуры
                { return check_shape; }
            }
        }
        return -1; // В ячейке нет фигур
    }

    void Update() // Сердце иры
    {
        if (stop_game) { return; } // Если игра остановлена, отдыхаем

        // Нажатие кнопок 
        if (Input.GetKeyDown(KeyCode.A)) { N_shape = 0; MoveAside(-1); } // Двигать фигуру влево
        if (Input.GetKeyDown(KeyCode.D)) { N_shape = 0; MoveAside(1); } // Двигать фигуру вправо
        if (Input.GetKeyDown(KeyCode.S) && shape[0].blocks_count < 16) { shape[0].drop_shape = true; } // Сброс фигуры вниз
        if (Input.GetKeyDown(KeyCode.W) && shape[0].can_rotate) { N_shape = 0; RotateShape(); } // Вращение фигуры по часовой
        if (Input.GetKeyDown(KeyCode.LeftArrow)) { N_shape = 1; MoveAside(-1); } // Двигать фигуру влево
        if (Input.GetKeyDown(KeyCode.RightArrow)) { N_shape = 1; MoveAside(1); } // Двигать фигуру вправо
        if (Input.GetKeyDown(KeyCode.DownArrow) && shape[1].blocks_count < 16) { shape[1].drop_shape = true; } // Сброс фигуры вниз
        if (Input.GetKeyDown(KeyCode.UpArrow) && shape[1].can_rotate) { N_shape = 1; RotateShape(); } // Вращение фигуры по часовой

        // Движение фигур по таймерам
        N_shape = 0; // Проверка опускания первой фигуры
        LowerShape();
        N_shape = 1;
        LowerShape(); // Проверка опускания второй фигуры
        N_shape = -1;
    }

    // Управление в мобильной версии
    public void LeftLeftButton_press() { if (!stop_game) { N_shape = 0; MoveAside(-1); } } // Двигать левую фигуру влево
    public void LeftRightButton_press() { if (!stop_game) { N_shape = 0; MoveAside(1); } } // Двигать левую фигуру вправо
    public void LeftUpButton_press() { if (!stop_game && shape[0].can_rotate) { N_shape = 0; RotateShape(); } } // Вращение левой фигуры по часовой
    public void LeftDownButton_press() { if (!stop_game && shape[0].blocks_count < 16) { shape[0].drop_shape = true; } } // Сброс левой фигуры
    public void RightLeftButton_press() { if (!stop_game) { N_shape = 1; MoveAside(-1); } } // Двигать правую фигуру влево
    public void RightRightButton_press() { if (!stop_game) { N_shape = 1; MoveAside(1); } } // Двигать правую фигуру вправо
    public void RightUpButton_press() { if (!stop_game && shape[1].can_rotate) { N_shape = 1; RotateShape(); } } // Вращение правой фигуры по часовой
    public void RightDownButton_press() { if (!stop_game && shape[1].blocks_count < 16) { shape[1].drop_shape = true; } } // Сброс правой фигуры

    private void MoveAside(int delta) // Движение фигуры в сторону. delta - смещение, если -1 это влево, если 1 то вправо
    {
        // Проверяем, есть ли место возле фигуры. Если нет, то прерываем работу функции и фигура не движется
        for (int i = 0; i < shape[N_shape].blocks_count; i++) // Цикл по блокам фигуры
        {
            int block_x = (int)(shape[N_shape].x + shape[N_shape].block_pos[i, 0]) + delta; // Координаты ячейки в которую хотим сместить блок фигуры. Нужны целые для обращения к массиву координат блоков
            int block_y = (int)(shape[N_shape].y + shape[N_shape].block_pos[i, 1]);
            if (block_x == 0 || block_x == 16) { return; } // Двигать нельзя, мешает стена
            if (space_cells_status[block_x, block_y] != "") { return; } // Один из блоков фигуры упёрся в какай-то блок стакана
            if (ShapeInCell(block_x, block_y) != -1 && ShapeInCell(block_x, block_y) != N_shape) { return; } // Двигать нельзя, мешает другая фигура
        }
        // Все проверки пройдены, ничто не мешает. Сдвигаем
        shape[N_shape].x = shape[N_shape].x + (float)delta;
        for (int i = 0; i < shape[N_shape].blocks_count; i++)
        {
            float new_X = shape[N_shape].x + shape[N_shape].block_pos[i, 0];
            float new_Y = shape[N_shape].y + shape[N_shape].block_pos[i, 1];
            shape[N_shape].block_sprite[i].transform.position = new Vector3(new_X, new_Y, 0); // Перемещение блока
        }
    }
    private void RotateShape() // Вращение фигуры. 
    {
        // Логика действия - При повороте фигуры меняем местами смещения по X и Y

        // Проверка, есть ли место для фигуры после разворота
        for (int i = 0; i < shape[N_shape].blocks_count; i++) // Цикл по блокам фигуры
        {
            int block_x = (int)(shape[N_shape].x + shape[N_shape].block_pos[i, 1]); // Координаты ячейки в которую хотим сместить блок фигуры. Нужны целые для обращения к массиву координат блоков
            int block_y = (int)(shape[N_shape].y - shape[N_shape].block_pos[i, 0]);
            if (block_x <= 0 || block_x >= 16 || block_y <= 0) { return; } // Стена мешает повороту
            if (space_cells_status[block_x, block_y] != "") { return; } // Один из блоков фигуры при повороте попадает на занятое место
            if (ShapeInCell(block_x, block_y) != -1 && ShapeInCell(block_x, block_y) != N_shape) { return; } // Повороту мешает другая фигура
        }
        // Все проверки пройдены, ничто не мешает. Крутим
        for (int i = 0; i < shape[N_shape].blocks_count; i++)
        {
            // Запоминаем исходные координаты блока фигуры. X и Y меняем местами, для этого нужна временная переменная, а с двумя ещё нагляднее
            float new_X = shape[N_shape].block_pos[i, 1];
            float new_Y = -1f * shape[N_shape].block_pos[i, 0];
            // Новые координаты блока фигуры
            shape[N_shape].block_pos[i, 0] = new_X;
            shape[N_shape].block_pos[i, 1] = new_Y;

            // Перемещение и поворот спрайта
            shape[N_shape].block_sprite[i].transform.position = new Vector3(shape[N_shape].x + new_X, shape[N_shape].y + new_Y, 0); // Перемещение
            if (shape[N_shape].can_rotate_sprite) { shape[N_shape].block_sprite[i].transform.Rotate(0, 0, -90); } // Если спрайты блоков поворачиваются. Это для сложных мозаек
            else if (shape[N_shape].block_id[i] > 25)
            {
                // Этот код для бетонных блоков 2x2. Задача - перекручивать id и спрайты блоков обратно, чтобы мозайка из спрайтов оставалась в вертикальном положении
                int shape_id = shape[N_shape].block_id[i]; // id спрайта угла повёрнутого блока 2x2
                if ((shape_id - 1) % 4 == 0) { shape_id = shape_id - 3; }
                else { shape_id = shape_id + 1; } // Меняем id спрайта, "прокрутив" его обратно против часовой
                shape[N_shape].block_id[i] = shape_id;
                shape[N_shape].block_sprite[i].GetComponent<SpriteRenderer>().sprite = sprite_block[shape_id]; // Меняем спрайт угла
            }
        }
    }

    
    
    private void LowerShape() // Спуск фигуры вниз по таймеру 
    {
        if (shape[N_shape].drop_shape) { shape[N_shape].nextTime = Time.time; } // Если идёт сброс фигуры, то игнорируем задерку времени
        if (Time.time >= shape[N_shape].nextTime)
        {
            // Проверяем поблочно, сначала не мешает ли другая фигура?
            for (int i = 0; i < shape[N_shape].blocks_count; i++) // Цикл по блокам фигуры
            {
                int check_x = (int)(shape[N_shape].x + shape[N_shape].block_pos[i, 0]); // X координата ячейки под блоком
                int check_y = (int)(shape[N_shape].y + shape[N_shape].block_pos[i, 1] - 1); // Y координата ячейки под блоком
                if (ShapeInCell(check_x, check_y) != -1 && ShapeInCell(check_x, check_y) != N_shape) // Под нами другая фигура,
                { shape[N_shape].nextTime = Time.time + shape[N_shape].speed / 2; return; } // Перемещаться туда пока нельзя. Попозже
            }
            // Проверка достижения дна
            if (IsShapeDown()) { FixShape(); return; } // Если фигура легла, переводим её блоки в ячейки стакана, удаляем её и создаём новую. Всё это в FixShape, отсюда выходим
            //CheckDown(); // Вызов функции, которая проверит легла ли фигура. И если легла, то удалит её.
            //f (N_shape == -1) { return; } // Фигура перешла в стакан и прекратила своё существование. Выходим из функции
            // Фигура ни на что не легла
            // Все проверки пройдены, можно опустить фигуру
            shape[N_shape].y--; // Сдвиг вниз центра центра фигуры
            for (int i = 0; i < shape[N_shape].blocks_count; i++) // Смещаем поблочно спрайты фигуры на один вниз
            {
                shape[N_shape].block_sprite[i].transform.position = new Vector3(shape[N_shape].x + shape[N_shape].block_pos[i, 0], shape[N_shape].y + shape[N_shape].block_pos[i, 1], 0); // Новая позиция спрайта
            }
            // Устанавливаем время следующего опускания фигуры
            shape[N_shape].nextTime = Time.time + shape[N_shape].speed;
        }

    }
    private bool IsShapeDown() // Проверка, опустилась ли фигура на дно или занятые ячейки стакана
    {
        for (int i = 0; i < shape[N_shape].blocks_count; i++) // Цикл по блокам фигуры. Проверка каждого, есть ли под ним пустое место для движения
        {
            int cell_x = (int)(shape[N_shape].x + shape[N_shape].block_pos[i, 0]); // индексы массива ячейки стакана, в которой остановился блок фигуры. Нужны целые.
            int cell_y = (int)(shape[N_shape].y + shape[N_shape].block_pos[i, 1]);
            if (cell_y == 1 || space_cells_status[cell_x, cell_y - 1] != "") // Если блок фигуры достиг дна или под ним занятая ячейка
            { return true; } // Возвращаем положительный ответ
        }
        return false; // Под всеми ячейками свободно, возвращаем отрицательный ответ - фигура не легла
    }

    private void FixShape() // Переход блоков фигуры в ячейки стакана
    {        
        sound_pong.Play();
        // Отмечаем ячейки стакана за фигурой как ЗАНЯТО и перекрашиваем
        for (int j = 0; j < shape[N_shape].blocks_count; j++) // цикл по блокам фигуры
        {
            int cell_x = (int)(shape[N_shape].x + shape[N_shape].block_pos[j, 0]); //координаты блока фигуры. Нужны целые для обращения к массиву ячеек стакана space_cells_status
            int cell_y = (int)(shape[N_shape].y + shape[N_shape].block_pos[j, 1]);

            switch (shape[N_shape].block_id[j])
            //Переводим id спрайта блока от упавшей фигуры в текстовое имя для ячейки стакана (для удобства программирования). 
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
                    FallDiamond(cell_x, cell_y); break; // Попробовать сразу вывести алмаз из игры. Линия не соберётся!
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
                        int randomLucky = UnityEngine.Random.Range(0, 2);  // Вероятность срабатывания бомбы
                        if (randomLucky != 0) { DamageCell2x2(cell_x, cell_y, 1); } // Если бомбу сбрасывают, она детонирует
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
                    FallDiamond2x2(cell_x, cell_y); break; // Попробовать сразу вывести алмаз из игры, если он лёг на дно.
                case 50:
                case 54:
                case 58:
                case 62:
                case 66:
                case 70:
                case 74:
                    space_cells_status[cell_x, cell_y] = "2x2 -l-d beton crack-00";
                    space_cells_sprite[cell_x, cell_y].GetComponent<SpriteRenderer>().sprite = sprite_block[30]; // Спрайт неповреждённого серого блока 2x2
                    break;
                case 51:
                case 55:
                case 59:
                case 63:
                case 67:
                case 71:
                case 75:
                    space_cells_status[cell_x, cell_y] = "2x2 -l-u beton crack-00";
                    space_cells_sprite[cell_x, cell_y].GetComponent<SpriteRenderer>().sprite = sprite_block[31]; // Спрайт неповреждённого серого блока 2x2
                    break;
                case 52:
                case 56:
                case 60:
                case 64:
                case 68:
                case 72:
                case 76:
                    space_cells_status[cell_x, cell_y] = "2x2 -r-u beton crack-00";
                    space_cells_sprite[cell_x, cell_y].GetComponent<SpriteRenderer>().sprite = sprite_block[32]; // Спрайт неповреждённого серого блока 2x2
                    break;
                case 53:
                case 57:
                case 61:
                case 65:
                case 69:
                case 73:
                case 77:
                    space_cells_status[cell_x, cell_y] = "2x2 -r-d beton crack-00";
                    space_cells_sprite[cell_x, cell_y].GetComponent<SpriteRenderer>().sprite = sprite_block[33]; // Спрайт неповреждённого серого блока 2x2
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
                        int randomLucky = UnityEngine.Random.Range(0, 2);  // Вероятность срабатывания бомбы
                        if (randomLucky != 0) { DamageCell2x2(cell_x, cell_y, 1); } // Если бомбу сбрасывают, она детонирует
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

        // Если сбросили фигуру на бомбу, то детонация
        if (shape[N_shape].drop_shape)
        {
            for (int j = 0; j < shape[N_shape].blocks_count; j++) // цикл по блокам фигуры
            {
                int cell_x = (int)(shape[N_shape].x + shape[N_shape].block_pos[j, 0]); //координаты блока фигуры. Нужны целые для обращения к массиву ячеек стакана space_cells_status
                int cell_y = (int)(shape[N_shape].y + shape[N_shape].block_pos[j, 1]);
                if (cell_y == 1) { break; } // Ниже дна бомб нет
                if (ShapeInCell(cell_x, cell_y - 1) != -1) { continue; } // Пропускаем блоки своей фигуры, иначе взывается бомба в ящике
                if (space_cells_status[cell_x, cell_y - 1].Contains("bomb")) 
                {
                    int randomLucky = UnityEngine.Random.Range(0, 2);  // Вероятность срабатывания бомбы
                    if (randomLucky != 0) { DamageCell2x2(cell_x, cell_y - 1, 1); } // Взрыв бомбы
                } 
            }
        }
        // Если это ключевая фигура уровня (для некоторых уровней), переключаем флаг vip_shape в "-1", что означает, что фигура перешла в стакан
        if (levelManager.vip_shape == N_shape) { levelManager.vip_shape = -1; }
        CheckSpace(); // Проверка заполнения линий
        DeleteShape(); // Удаляем фигуру
        levelManager.shapes_count++; // Увеличиваем счётчик пройденных фигур
        levelManager.SetNewShape(); // Выбор следующей фигуры в менеджере
        return; // Фигура зафиксирована, остальные блоки проверять не нужно, выходим
    }
    private void DeleteShape() // Удаление фигуры
    {
        for (int i = 0; i < shape[N_shape].blocks_count; i++) // Удаляем спрайты упавшей фигуры
        {
            Destroy(shape[N_shape].block_sprite[i]);
        }
    }

    private void CheckSpace() // Проверка заполнения всех линий стакана
    {
        for (int y_check = 40; y_check > 0; y_check--) // Проходим по всем линиям стакана сверху вниз
        {
            CheckLine(y_check);
        }
    }
    private void CheckLine(int y) // Проверка одной линии 
    {
        while (true) // Эта линия может собираться не один раз за счёт опускающихся блоков, например алмаз, упавший в "колодец". Будем проверять пока собирается
        {
            // Проверяем, заполнена ли линия
            for (int x = 1; x < 16; x++) // Смотрим циклом ячейки
            {
                if (space_cells_status[x, y] == "") { return; } // Минимум одна ячейка не заполнена. Линия не собрана, выходим из проверки этой линии
            }
            // Линия собрана
            sound_crash.Play();
            for (int x = 1; x < 16; x++) // Проходим по ячейкам вдоль удаляемой линии
            {
                // Сначала наносим урон по ячейке
                if (space_cells_status[x, y].Contains("2x2")) // Это блок ячеек 2x2
                {
//                    if (space_cells_status[x, y].Contains("bomb")) { DamageCell2x2(x, y); } // Если это бомба, то будет взрыв и второй блок бомбы исчезнет, дамажить его не нужно. Смещения верхних ячеек тоже пройдут в функции DamageCell2x2
//                    else if (!space_cells_status[x, y].Contains("diamond")) // Алмаз не разрушим, пропускаем
//                    {
                        DamageCell2x2(x, y, 2);  // Нанесение урона =2 по блоку ячеек 2x2
//                        if (space_cells_status[x, y] == "") // Блок ячеек 2x2 разрушен
//                        {
//                            ShiftColumn(x, y_for_shift); // Cпускаем колонки ячеек на место уничтоженного блока ячеек 2x2
//                            ShiftColumn(x + 1, y_for_shift);
//                            ShiftColumn(x, y_for_shift);
//                            ShiftColumn(x + 1, y_for_shift);
//                        }
                        x++; // Пропускаем вторую ячейку блока 2x2
                }
                else
                {
                    DamageCell(x, y, 1); // Нанесение урона по одиночной ячейке
//                    ShiftColumn(x, y); // Cпускаем колонку ячеек сверху на место собранной линии
                }
            }
            /*
             * При совмещении вызовов Урон+СпускКолонки остаётся эффект, который легче принять, чем корректировать.
             * Если в собранной линии окажется бомба, то в левую сторону она нанесёт урон по уже спустившимся ячейкам.
             * Можно разделить Урон и СпускКолонки, но из-за блоков ячеек 2x2 придётся вставлять много костылей. Например, задержку взрыва(ов) до проверки всей линии
            */
        }
    }
    private void DamageCell(int x, int y, int power, bool shift = true) // Урон по ячейке стакана
    {
        if (space_cells_status[x, y].Contains("2x2")) { DamageCell2x2(x, y, power, shift);  return; } // Для блоков 2x2 создана отдельная функция

        switch (space_cells_status[x, y]) // Смотрим что за блок в ячейке
        {
            case "beton": // Ломаем бетон
                if (power == 1)
                {
                    space_cells_status[x, y] = "cracked beton";
                    space_cells_sprite[x, y].GetComponent<SpriteRenderer>().sprite = sprite_block[17]; // Спрайт потресканного бетона
                }
                else // Урон достаточный, чтобы разрушить этот бетонный блок
                {
                    space_cells_status[x, y] = "";
                    space_cells_sprite[x, y].GetComponent<SpriteRenderer>().sprite = sprite_background; // Пустой спрайт
                }
                break;
            case "diamond": // Алмаз не разрушим
                FallDiamond(x, y); // Но может упасть, если взрывом сожжёт его подпорку
                break;
            default: // Разрушаемые блоки освобождаем
                space_cells_status[x, y] = "";
                space_cells_sprite[x, y].GetComponent<SpriteRenderer>().sprite = sprite_background; // Пустой спрайт
                break;
        }
        if (shift) { ShiftColumn(x, y); } // Если в аргументах задано, но сдвигаем колонку на место уничтоженной ячейки
    }
    private void ShiftColumn(int target_x, int target_y) // Смещение колонки ячеек вниз к указанной ячейке
    {
        int y = target_y; // Счётчик для цикла перебора ячеек вверх
        while ( y < 40 ) // Снизу вверх от удаляемой линии "сдвигаем" все значения ячеек вниз
        {
            // Проверка ячейки, которую пробуем опустить
            if (space_cells_status[target_x, y] == "") // Сдвигаем в пустые ячейки. Занятые пропускаем, но не прекращаем сдвиг. Выход только при контакте с блоком ячеек 2x2
            {
                if (space_cells_status[target_x, y + 1].Contains("2x2")) // Сверху лежит блок ячеек 2x2
                {
                    CheckBombTouch(target_x, y + 1); // Проверяем, нет ли там бомбы. Если есть, то взрыв
                    space_cells_status[target_x, y] = ""; 
                    space_cells_sprite[target_x, y].GetComponent<SpriteRenderer>().sprite = sprite_background;
                    if (space_cells_status[target_x, y + 1].Contains("diamond")) // Это алмаз 2x2
                    {
                        FallDiamond2x2(target_x, y + 1);
                        return;
                    }
                    else if (space_cells_status[target_x, y + 1].Contains("-r-d"))  //  Сверху правый угол блока 2x2, то будем пробовать его сдвинить в функции для 2x2 ячеек, отсюда выходим
                    {
                        ShiftBlock2x2(target_x, y);
                        return;
                    }
                    // Если это левый угол блока 2x2, то пока ничего не делаем. Этот блок обработается, когда дойдёт очередь до правого нижнего
                    return;
                }

                // Смещение ячейки сверху
                if (space_cells_status[target_x, y] != "" && ShapeInCell(target_x, y) != -1) { shape[ShapeInCell(target_x, y)].drop_shape = true; } // Если при смещении прижмёт другую фигуру, ей сброс
                space_cells_status[target_x, y] = space_cells_status[target_x, y + 1]; // Статус ячейки присваевается от ячейки сверху
                space_cells_sprite[target_x, y].GetComponent<SpriteRenderer>().sprite = space_cells_sprite[target_x, y + 1].GetComponent<SpriteRenderer>().sprite; // То же со спрайтом ячейки
                space_cells_status[target_x, y + 1] = ""; // Статус ячейки сверху нужно обнулять
            }
            y++;
        }
        // Смотрим алмаз в ячейке после сдвига.
        if (space_cells_status[target_x, target_y] == "diamond") // На опустившую ячейку пустился алмазик, пробуем пропустить его дальше вниз
        { FallDiamond(target_x, target_y); }
    }
    private void FallDiamond(int x, int y) // Падение неразрушаемого объекта (алмаз) после удаления блока под ним
    {
        while (y > 1) // Пока не дойдём до дна
        {
            if (space_cells_status[x, y - 1] != "" ) // Если внизу занятая ячейка
            {
                // Останавливаем спуск
                CheckSpace(); // Требуется дополнительно проверить, не собрались ли при этом линия. Можно было бы только одну линию, но для упрощения кода проверим все
                return; // И выходим
            }
            // Если внизу свободная ячейка, смещаем на её место алмаз и колонку ячеек на нём
            if (ShapeInCell(x, y - 1) != -1) { shape[ShapeInCell(x, y - 1)].drop_shape = true; } // Если при смещении прижмёт другую фигуру, ей сброс
            ShiftColumn(x, y - 1);
            y--;
        }
        // Дошли до дна стакана. Опускаться дальше некуда
        // Алмаз выходит со сцены 
        space_cells_status[x, 1] = "";
        ShiftColumn(x, 1); // Сдвигаем колонку ячеек на место вышедшего алмаза
    }




    // Блоки 2x2
    private void DamageCell2x2(int x, int y, int power, bool shift = true) // Урон по блоку ячеек 2x2 стакана, урон получают все четыре ячейки
    {
        if (!space_cells_status[x, y].Contains("2x2")) { Debug.Log("ALARM! в DamageCell2x2 наносится урон по " + space_cells_status[x, y] + " (" + x + ":" + y + ")"); Debug.Break();  return; } // ячейка 2x2 уже разрушена
        // Определение правого нижнего угла
        switch (space_cells_status[x, y].Substring(4, 4)) // Находим правый нижний угол
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
        if (space_cells_status[x, y].Contains("diamond")) { FallDiamond2x2(x, y); return; } // Алмаз не разрушим, но может спуститься вниз если под ним стало пусто
        else if (space_cells_status[x, y].Contains("bombWhite")) { Flash2x2(x, y); return; } // Взрыв белой бомбы вспышки
        else if (space_cells_status[x, y].Contains("bombBlack")) { Blast2x2(x, y); return; } // Взрыв чёрной бомбы c дымом и обвалом
        else if (space_cells_status[x, y].Contains("beton"))
        {
            // бетонные 2x2 с 16-тью жизнями
            int startIndex = space_cells_status[x, y].LastIndexOf('-') + 1; // Положение символа "-" в статусе ячейки
            string numberString = space_cells_status[x, y].Substring(startIndex); // Текущее здоровье ячейки (сколько уже есть повреждений) в текстовом формате
            int count_damage = int.Parse(numberString); // Конвертация в число
                                                        // Увеличиваем число повреждений блока ячейки на 1
            count_damage = count_damage + power;
            // Заменяем числа в строках на новое значение
            if (count_damage < 16)
            {
                space_cells_status[x - 1, y] = space_cells_status[x - 1, y].Substring(0, startIndex) + count_damage.ToString();
                space_cells_sprite[x - 1, y].GetComponent<SpriteRenderer>().sprite = sprite_block[34 + Mathf.FloorToInt(count_damage / 4) * 4]; // Спрайт потресканного левого нижнего угла блока 2х2
                space_cells_status[x - 1, y + 1] = space_cells_status[x - 1, y + 1].Substring(0, startIndex) + count_damage.ToString();
                space_cells_sprite[x - 1, y + 1].GetComponent<SpriteRenderer>().sprite = sprite_block[35 + Mathf.FloorToInt(count_damage / 4) * 4]; // Спрайт потресканного левого верхнего угла блока 2х2
                space_cells_status[x, y + 1] = space_cells_status[x, y + 1].Substring(0, startIndex) + count_damage.ToString();
                space_cells_sprite[x, y + 1].GetComponent<SpriteRenderer>().sprite = sprite_block[36 + Mathf.FloorToInt(count_damage / 4) * 4]; // Спрайт потресканного правого верхнего угла блока 2х2
                space_cells_status[x, y] = space_cells_status[x, y].Substring(0, startIndex) + count_damage.ToString();
                space_cells_sprite[x, y].GetComponent<SpriteRenderer>().sprite = sprite_block[37 + Mathf.FloorToInt(count_damage / 4) * 4]; // Спрайт потресканного правого нижнего угла блока 2х2
            }
            else { clearCell2x2(x, y, shift); } // Блок разрушен
        }
        else if (space_cells_status[x, y].Contains("gorlum"))
        {
            // Горлум
            int startIndex = space_cells_status[x, y].LastIndexOf('-') + 1; // Положение символа "-" в статусе ячейки
            string numberString = space_cells_status[x, y].Substring(startIndex); // Текущее здоровье ячейки (сколько уже есть повреждений) в текстовом формате
            int count_damage = int.Parse(numberString); // Конвертация в число
                                                        // Увеличиваем число повреждений блока ячейки на 1
            count_damage++;
            // Заменяем числа в строках на новое значение
            if (count_damage < 40)
            {
                space_cells_status[x - 1, y] = space_cells_status[x - 1, y].Substring(0, startIndex) + count_damage.ToString();
                space_cells_status[x - 1, y + 1] = space_cells_status[x - 1, y + 1].Substring(0, startIndex) + count_damage.ToString();
                space_cells_status[x, y + 1] = space_cells_status[x, y + 1].Substring(0, startIndex) + count_damage.ToString();
                space_cells_status[x, y] = space_cells_status[x, y].Substring(0, startIndex) + count_damage.ToString();
            }
            else { clearCell2x2(x, y, shift); } // Блок разрушен
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


    private void ShiftBlock2x2(int free_x, int free_y) // Смещение блока 2x2 к указанной ячейке (правым углом)
    {
        if (space_cells_status[free_x - 1, free_y] != "" ||
            space_cells_status[free_x, free_y] != "") { return; } // Под блока ячеек 2x2 должно быть свободно
        // Переносим ячейки блока 2x2 на одну клетку вниз
        if (ShapeInCell(free_x, free_y - 1) != -1) { shape[ShapeInCell(free_x, free_y - 1)].drop_shape = true; } // Если при смещении слева прижмёт другую фигуру, ей сброс
        if (ShapeInCell(free_x - 1, free_y - 1) != -1) { shape[ShapeInCell(free_x - 1, free_y - 1)].drop_shape = true; } // Если при смещении справа прижмёт другую фигуру, ей сброс
        space_cells_status[free_x - 1, free_y] = space_cells_status[free_x - 1, free_y + 1];
        space_cells_status[free_x, free_y] = space_cells_status[free_x, free_y + 1];
        space_cells_status[free_x - 1, free_y + 1] = space_cells_status[free_x - 1, free_y + 2];
        space_cells_status[free_x, free_y + 1] = space_cells_status[free_x, free_y + 2];
        space_cells_status[free_x - 1, free_y + 2] = "";
        space_cells_status[free_x, free_y + 2] = "";
        // И спрайты
        space_cells_sprite[free_x - 1, free_y].GetComponent<SpriteRenderer>().sprite = space_cells_sprite[free_x - 1, free_y + 1].GetComponent<SpriteRenderer>().sprite;
        space_cells_sprite[free_x, free_y].GetComponent<SpriteRenderer>().sprite = space_cells_sprite[free_x, free_y + 1].GetComponent<SpriteRenderer>().sprite;
        space_cells_sprite[free_x - 1, free_y + 1].GetComponent<SpriteRenderer>().sprite = space_cells_sprite[free_x - 1, free_y + 2].GetComponent<SpriteRenderer>().sprite;
        space_cells_sprite[free_x, free_y + 1].GetComponent<SpriteRenderer>().sprite = space_cells_sprite[free_x, free_y + 2].GetComponent<SpriteRenderer>().sprite;

        ShiftColumn(free_x - 1, free_y + 2); // Спускаем левую колонку ячеек над блоком 2x2
        ShiftColumn(free_x, free_y + 2); // Спускаем правую колонку ячеек над блоком 2x2
    }
    private void FallDiamond2x2(int x, int y) // Падение алмаза 2x2 после удаления блока под ним. Аргументы - координаты правого нижнего угла
    {
        // Корректировка угла
        if (!space_cells_status[x, y].Contains("-r-d")) { x = x + 1; } // Значит функция вызвалась на левом углу
        while (y > 1) // Пока не дойдём до дна
        {
            if (space_cells_status[x, y - 1] != "" ||
                space_cells_status[x - 1, y - 1] != "")
            {
                // Останавливаем спуск
                CheckSpace(); // Проверяем собранные линии
                return; // И выходим
            }
            // А если внизу обе ячейки свободны, спускаем этот блок ячеек
            // Переносим ячейки блока 2x2 на одну клетку вниз
            ShiftBlock2x2(x, y - 1);
            y--;
        }
        // Дошли до дна стакана. Опускаться дальше некуда
        // Алмаз выходит со сцены 
        space_cells_status[x, y] = "";
        space_cells_status[x - 1, y] = "";
        space_cells_status[x, y + 1] = "";
        space_cells_status[x - 1, y + 1] = "";
        ShiftBlock2x2(x, y); // Это уже не принципиально
        ShiftBlock2x2(x + 1, y);
        ShiftBlock2x2(x, y);
        ShiftBlock2x2(x + 1, y + 1);
    }




    // БОМБА
    private void CheckBombTouch(int x, int y) // Проверка, не собрана ли линия под бомбой? Это может вызвать сдвиг половины бомбы, некрасиво разорвётся спрайт и бомба взрывается :)
    {
        if (space_cells_status[x, y].Contains("bomb")) { DamageCell2x2(x, y, 1); }
    }
    private void Flash2x2(int x, int y) // Вспышка бомбы 2x2 без дыма и обвалов
    {
        Instantiate(prefabFlash, new Vector3(x - 0.5f, y + 0.5f, 0), Quaternion.identity); // Анимация вспышки

        // Очищаем значение в месте взрыва, чтобы не вызвать бесконечный цикл из-за детонации соседних бомб
        space_cells_status[x - 1, y] = "";
        space_cells_sprite[x - 1, y].GetComponent<SpriteRenderer>().sprite = sprite_background; // пустой спрайт
        space_cells_status[x, y] = "";
        space_cells_sprite[x, y].GetComponent<SpriteRenderer>().sprite = sprite_background;
        space_cells_status[x - 1, y + 1] = "";
        space_cells_sprite[x - 1, y + 1].GetComponent<SpriteRenderer>().sprite = sprite_background;
        space_cells_status[x, y + 1] = "";
        space_cells_sprite[x, y + 1].GetComponent<SpriteRenderer>().sprite = sprite_background;
        // Выжигание ячеек
        int power = 3; // Сила взрыва
        // Форма взрыва - ромб с радиусом, равным силе взрыва. Но вокруг блока ячеек 2x2, x:y - позиция центра взрыва.
        float x_center = x - 0.5f; float y_center = y + 0.5f; // пересчёт координат центра взрыва от правого нижнего угла бомбы
        for (float burnedCell_x = x_center - power - 0.5f; burnedCell_x <= x_center + power + 0.5f; burnedCell_x++) // Внешний цикл по горизонтали
        {
            if (burnedCell_x < 1 || burnedCell_x > (16 - 1)) { continue; } // Если по горизонтали вышло за экран, то пропускаем
            // Высота ромба на этом удалении от центра взрыва. Максимум в центре
            float x_power = power - Mathf.Abs(x_center - burnedCell_x + 0.5f * (burnedCell_x - x_center) / Mathf.Abs(burnedCell_x - x_center)); // Высота ромба на этом удалении от центра взрыва. Максимум в центре

            for (float burnedCell_y = y_center + x_power + 0.5f; burnedCell_y >= y_center - x_power - 0.5f; burnedCell_y--) // Внешний цикл по вертикали. Проходим сверху вниз, чтобы не уничтожались ячейки, опускающиеся на место сгоревших 2x2. 
            {
                if (burnedCell_y < 1 || burnedCell_y > 40) { continue; } // Если по вертикали вышло за экран, то пропускаем
                int damage_power = (int)(x_power - Mathf.Abs(y_center - burnedCell_y) + 0.5f + 1f); // Количество урона зависит от расстояния ячейки от центра взрыва
                if (space_cells_status[(int)burnedCell_x, (int)burnedCell_y] != "")
                {
                    DamageCell((int)burnedCell_x, (int)burnedCell_y, damage_power);   // Нанесение урона по ячейке. Разрушения без сдвига вниз
                }
            }
        }
    }
    private void Blast2x2(int x, int y) // Взрыв бомбы 2x2 с дымом
    {
        // Очищаем значения в месте взрыва, чтобы не вызвать бесконечный цикл из-за детонации соседних бомб
        space_cells_status[x - 1, y] = "";
        space_cells_sprite[x - 1, y].GetComponent<SpriteRenderer>().sprite = sprite_background; // пустой спрайт
        space_cells_status[x, y] = "";
        space_cells_sprite[x, y].GetComponent<SpriteRenderer>().sprite = sprite_background;
        space_cells_status[x - 1, y + 1] = "";
        space_cells_sprite[x - 1, y + 1].GetComponent<SpriteRenderer>().sprite = sprite_background;
        space_cells_status[x, y + 1] = "";
        space_cells_sprite[x, y + 1].GetComponent<SpriteRenderer>().sprite = sprite_background;
        // Выжигание ячеек
        int power = 4; // Сила взрыва
        // Форма взрыва - ромб с радиусом, равным силе взрыва. Но вокруг блока ячеек 2x2, x:y - позиция центра взрыва.
        float x_center = x - 0.5f; float y_center = y + 0.5f; // пересчёт координат центра взрыва от правого нижнего угла бомбы
        for (float burnedCell_x = x_center - power - 0.5f; burnedCell_x <= x_center + power + 0.5f; burnedCell_x++) // Внешний цикл по горизонтали
        {
            if (burnedCell_x < 1 || burnedCell_x > (16 - 1)) { continue; } // Если по горизонтали вышло за экран, то пропускаем
            // Высота ромба на этом удалении от центра взрыва. Максимум в центре
            float x_power = power - Mathf.Abs(x_center - burnedCell_x + 0.5f * (burnedCell_x - x_center) / Mathf.Abs(burnedCell_x - x_center)); // Высота ромба на этом удалении от центра взрыва. Максимум в центре

            for (float burnedCell_y = y_center + x_power + 0.5f; burnedCell_y >= y_center - x_power - 0.5f; burnedCell_y--) // Внешний цикл по вертикали. Проходим сверху вниз, чтобы не уничтожались ячейки, опускающиеся на место сгоревших 2x2. 
            {
                if (burnedCell_y < 1 || burnedCell_y > 40) { continue; } // Если по вертикали вышло за экран, то пропускаем
                int damage_power = (int)(x_power - Mathf.Abs(y_center - burnedCell_y) + 0.5f + 1f); // Количество урона зависит от расстояния ячейки от центра взрыва
                if (space_cells_status[(int)burnedCell_x, (int)burnedCell_y] != "") 
                {
                    DamageCell((int)burnedCell_x, (int)burnedCell_y, damage_power, true);   // Нанесение урона по ячейке. Разрушения со сдвигом вниз
                }
                ShiftColumn((int)burnedCell_x, (int)burnedCell_y); // Обвал ячеек из-за взрыва
                GameObject smoke = Instantiate(prefabSmoke, new Vector3(burnedCell_x, burnedCell_y, 0), Quaternion.identity); // Создаем экземпляр префаба дыма
                SpriteRenderer spriteRenderer = smoke.GetComponent<SpriteRenderer>(); // Получаем его SpriteRenderer
                spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, (float)damage_power / (float)(power - 0.001f) * 1.5f); // Установка густоты дыма в зависимости от удаления от взрыва
            }
        }
        sound_boom.Play(); // Звук взрыва
        CheckSpace(); // Проверка линий, собравшихся из-за обрушений
    }




    // СОЗДАНИЕ ФИГУР

    private void SetShapeDefaultValues() // Значения по-умолчанию для классической фигуры
    {
        // Координаты центра фигуры по-умолчанию
        if (N_shape == 0) { shape[N_shape].x = 3f; }
        else { shape[N_shape].x = 12f; }
        shape[N_shape].y = 19f;

        shape[N_shape].block_id = new int[shape[N_shape].blocks_count]; // Создаём массив id спрайтов блоков фигуры
        shape[N_shape].block_sprite = new GameObject[shape[N_shape].blocks_count];  // Создаём массив самих спрайтов (объектов)
        shape[N_shape].speed = 1f;
        shape[N_shape].can_rotate = true;
        shape[N_shape].can_rotate_sprite = false;
}

    public void MakeNewClassicShape() // Создание новое фигуры классического тетриса. Аргумент count_beton - фича, добавляются бетонные блоки, которые разрушаются с двух сборок
    {
        shape[N_shape] = new TetrixShape();
        shape[N_shape].blocks_count = 4;
        SetShapeDefaultValues();
        int randomShape = UnityEngine.Random.Range(0, 7);  // Рандомно выбираем одну из 7 возможных стандартных фигурок
        //randomShape = 5;
        switch (randomShape)
        {
            case 0: // "O"
                // Массив позиций блоков, относительных центра
                shape[N_shape].block_pos = new float[,] { { -0.5f, -0.5f }, { -0.5f, 0.5f }, { 0.5f, 0.5f }, { 0.5f, -0.5f } };
                shape[N_shape].x = shape[N_shape].x + 1.5f; shape[N_shape].y = 18.5f; // у кубика нет блока в середине
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
        for (int i = 0; i < shape[N_shape].blocks_count; i++) // Всем блокам присваиваем одинаковые id спрайтов
        {
            shape[N_shape].block_id[i] =  randomShape + 1;
        }
    }
    public void MakeBetonBlock(int count_beton)
    {
        /* Фича
        count_beton - количество бетонных блоков,
        меняем у случайного блока id на бетонный такого же цвета.
        Повторяем это count_beton раз. 
        */
        MakeNewClassicShape();
        for (int i = 0; i < count_beton; i++)
        {
            while (true)
            {
                int randomId = UnityEngine.Random.Range(0, 4);  //  Случайное число от 0 до 3 (включительно)
                if (shape[N_shape].block_id[randomId] < 9) // Бетонный блок должен встать вместь обычного, крутим циклом пока не выпадет обычный
                {
                    shape[N_shape].block_id[randomId] = shape[N_shape].block_id[randomId] + 9; // id бетонного блока
                    break;
                }
            }
        }
    }
    public void MakeClassicShape2x2(int mode) // Создание большой тетрис фигуры из блоков 2x2
    {
        shape[N_shape].blocks_count = 16;
        SetShapeDefaultValues();
        int ld_id = 0; // id спрайта левого нижнего угла. Для разных цветов фигур
        switch (mode)
        {
            case 0:  // "O"
                shape[N_shape].block_pos = new float[,] { { -1.5f, -1.5f }, { -1.5f, -0.5f }, { -0.5f, -0.5f }, { -0.5f, -1.5f },
                    { -1.5f, 0.5f }, { -1.5f, 1.5f }, { -0.5f, 1.5f }, { -0.5f, 0.5f },
                    { 0.5f, 0.5f }, { 0.5f, 1.5f }, { 1.5f, 1.5f }, { 1.5f, 0.5f },
                    { 0.5f, -1.5f }, { 0.5f, -0.5f }, { 1.5f, -0.5f }, { 1.5f, -1.5f }}; // Массив позиций блоков, относительных центра
                shape[N_shape].can_rotate = false;
                ld_id = 50;  break;
            case 1:  // "S"
                shape[N_shape].block_pos = new float[,] { { -2.5f, -1.5f }, { -2.5f, -0.5f }, { -1.5f, -0.5f }, { -1.5f, -1.5f },
                    { -0.5f, -1.5f }, { -0.5f, -0.5f }, { 0.5f, -0.5f }, { 0.5f, -1.5f },
                    { -0.5f, 0.5f }, { -0.5f, 1.5f }, { 0.5f, 1.5f }, { 0.5f, 0.5f },
                    { 1.5f, 0.5f }, { 1.5f, 1.5f }, { 2.5f, 1.5f }, { 2.5f, 0.5f }}; // Массив позиций блоков, относительных центра
                ld_id = 54; break;
            case 2:  // "Z"
                shape[N_shape].block_pos = new float[,] { { -2.5f, 0.5f }, { -2.5f, 1.5f }, { -1.5f, 1.5f }, { -1.5f, 0.5f },
                    { -0.5f, -1.5f }, { -0.5f, -0.5f }, { 0.5f, -0.5f }, { 0.5f, -1.5f },
                    { -0.5f, 0.5f }, { -0.5f, 1.5f }, { 0.5f, 1.5f }, { 0.5f, 0.5f },
                    { 1.5f, -1.5f }, { 1.5f, -0.5f }, { 2.5f, -0.5f }, { 2.5f, -1.5f }}; // Массив позиций блоков, относительных центра
                ld_id = 58; break;
            case 3:  // "I"
                shape[N_shape].block_pos = new float[,] { { -0.5f, -3.5f }, { -0.5f, -2.5f }, { 0.5f, -2.5f }, { 0.5f, -3.5f },
                    { -0.5f, -1.5f }, { -0.5f, -0.5f }, { 0.5f, -0.5f }, { 0.5f, -1.5f },
                    { -0.5f, 0.5f }, { -0.5f, 1.5f }, { 0.5f, 1.5f }, { 0.5f, 0.5f },
                    { -0.5f, 2.5f }, { -0.5f, 3.5f }, { 0.5f, 3.5f }, { 0.5f, 2.5f }}; // Массив позиций блоков, относительных центра
                ld_id = 62; break;
            case 4:  // "L"
                shape[N_shape].block_pos = new float[,] { { -1.5f, -2.5f }, { -1.5f, -1.5f }, { -0.5f, -1.5f }, { -0.5f, -2.5f },
                    { 0.5f, -2.5f }, { 0.5f, -1.5f }, { 1.5f, -1.5f }, { 1.5f, -2.5f },
                    { -1.5f, -0.5f }, { -1.5f, 0.5f }, { -0.5f, 0.5f }, { -0.5f, -0.5f },
                    { -1.5f, 1.5f }, { -1.5f, 2.5f }, { -0.5f, 2.5f }, { -0.5f, 1.5f }}; // Массив позиций блоков, относительных центра
                ld_id = 66; break;
            case 5:  // "J"
                shape[N_shape].block_pos = new float[,] { { -1.5f, -2.5f }, { -1.5f, -1.5f }, { -0.5f, -1.5f }, { -0.5f, -2.5f },
                    { 0.5f, -2.5f }, { 0.5f, -1.5f }, { 1.5f, -1.5f }, { 1.5f, -2.5f },
                    { 0.5f, -0.5f }, { 0.5f, 0.5f }, { 1.5f, 0.5f }, { 1.5f, -0.5f },
                    { 0.5f, 1.5f }, { 0.5f, 2.5f }, { 1.5f, 2.5f }, { 1.5f, 1.5f }}; // Массив позиций блоков, относительных центра
                ld_id = 70; break;
            case 6:  // "T"
                shape[N_shape].block_pos = new float[,] { { -2.5f, -1.5f }, { -2.5f, -0.5f }, { -1.5f, -0.5f }, { -1.5f, -1.5f },
                    { -0.5f, -1.5f }, { -0.5f, -0.5f }, { 0.5f, -0.5f }, { 0.5f, -1.5f },
                    { -0.5f, 0.5f }, { -0.5f, 1.5f }, { 0.5f, 1.5f }, { 0.5f, 0.5f },
                    { 1.5f, -1.5f }, { 1.5f, -0.5f }, { 2.5f, -0.5f }, { 2.5f, -1.5f }}; // Массив позиций блоков, относительных центра
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
    public void MakeDiamond() // Создание алмаза
    {
        shape[N_shape].blocks_count = 1;
        SetShapeDefaultValues();
        shape[N_shape].block_pos = new float[,] { { 0f, 0f } }; // Массив из единственной позиции блока, относительно центра
        shape[N_shape].can_rotate = false;
        shape[N_shape].block_id[0] = 8; // Картинка азмазик
    }
    public void MakeDiamond2x2() // Создание алмаза 2x2
    {
        shape[N_shape].blocks_count = 24;
        SetShapeDefaultValues();
        shape[N_shape].x = 10.5f; shape[N_shape].y = 30.5f;
        shape[N_shape].block_pos = new float[,] { { -1.5f, -1.5f }, { -1.5f, -0.5f }, { -0.5f, -0.5f }, { -0.5f, -1.5f },
                    { -1.5f, 0.5f }, { -1.5f, 1.5f }, { -0.5f, 1.5f }, { -0.5f, 0.5f },
                    { 0.5f, 0.5f }, { 0.5f, 1.5f }, { 1.5f, 1.5f }, { 1.5f, 0.5f },
                    { 0.5f, -1.5f }, { 0.5f, -0.5f }, { 1.5f, -0.5f }, { 1.5f, -1.5f },
                    { -0.5f, 2.5f }, { -0.5f, 3.5f }, { 0.5f, 3.5f }, { 0.5f, 2.5f },
                    { -0.5f, 4.5f }, { -0.5f, 5.5f }, { 0.5f, 5.5f }, { 0.5f, 4.5f },}; // Массив позиций блоков, относительных центра
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
            shape[N_shape].block_id[i] = 86 + i - 16; // Мозайка из спрайтов горлума
        }
        for (int i = 20; i < shape[N_shape].blocks_count; i++)
        {
            shape[N_shape].block_id[i] = 26 + i - 20; // Мозайка из спрайтов алмаза
        }
    }
    public void MakeBoxBombBlack() // Создание коробки с чёрной бомбой
    {
        shape[N_shape].blocks_count = 4;
        SetShapeDefaultValues();
        shape[N_shape].block_pos = new float[,] { { -1f, 0f }, { -1f, 1f }, { 0f, 1f }, { 0f, 0f }}; // Массив позиций блоков, относительных центра
        shape[N_shape].can_rotate = false;
        for (int i = 0; i < shape[N_shape].blocks_count; i++)
        {
            shape[N_shape].block_id[i] = 22 + i; // Мозайка из спрайтов ящика
        }
    }
    public void MakeBombBlack() // Создание чёрной бомбы без коробки
    {
        shape[N_shape].blocks_count = 4;
        SetShapeDefaultValues();
        shape[N_shape].block_pos = new float[,] { { -1f, 0f }, { -1f, 1f }, { 0f, 1f }, { 0f, 0f } }; // Массив позиций блоков, относительных центра
        shape[N_shape].can_rotate = false;
        for (int i = 0; i < shape[N_shape].blocks_count; i++)
        {
            shape[N_shape].block_id[i] = 18 + i; // Мозайка из спрайтов чёрной бомбы
        }
    }
    public void MakeBoxBombWhite() // Создание коробки с белой бомбой
    {
        shape[N_shape].blocks_count = 4;
        SetShapeDefaultValues();
        shape[N_shape].block_pos = new float[,] { { -1f, 0f }, { -1f, 1f }, { 0f, 1f }, { 0f, 0f } }; // Массив позиций блоков, относительных центра
        shape[N_shape].can_rotate = false;
        for (int i = 0; i < shape[N_shape].blocks_count; i++)
        {
            shape[N_shape].block_id[i] = 82 + i; // Мозайка из спрайтов ящика
        }
    }
    public void MakeBombWhite() // Создание белой бомбы без коробки
    {
        shape[N_shape].blocks_count = 4;
        SetShapeDefaultValues();
        shape[N_shape].block_pos = new float[,] { { -1f, 0f }, { -1f, 1f }, { 0f, 1f }, { 0f, 0f } }; // Массив позиций блоков, относительных центра
        shape[N_shape].can_rotate = false;
        for (int i = 0; i < shape[N_shape].blocks_count; i++)
        {
            shape[N_shape].block_id[i] = 78 + i; // Мозайка из спрайтов белой бомбы
        }
    }
}
