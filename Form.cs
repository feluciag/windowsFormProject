using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Runtime.Remoting.Contexts;
using static System.Data.Entity.Infrastructure.Design.Executor;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace TestWinApp
{
    public partial class Form1 : Form
    {
        private CoreEntityContext _context;
        public Form1()
        {
            InitializeComponent();
            _context = new CoreEntityContext("CoreEntityConnection");

            InitRootGroup();
            группуToolStripMenuItem.Click += группуToolStripMenuItem_Click;
            свойствоToolStripMenuItem.Click += свойствоToolStripMenuItem_Click;
            редактироватьToolStripMenuItem.Click += редактироватьToolStripMenuItem_Click;
            удалитьToolStripMenuItem.Click += удалитьToolStripMenuItem_Click;
            button1.Click += button1_Click;
            button2.Click += button2_Click;
           button3.Click += button3_Click;
            button4.Click += button4_Click;
            FormClosing += Form1_FormClosing;
        }

        private void InitRootGroup()
        {
            var rootGroup = _context.TGroupsProperty.FirstOrDefault(g => g.Id == 1);

            if (rootGroup == null)
            {
                MessageBox.Show("Корневая группа не найдена в базе данных.");
                return;
            }

            var rootNode = CreateNode(rootGroup);
            treeView1.Nodes.Add(rootNode);


            AddChildNodes(rootNode);
        }


        private TreeNode CreateNode(TGroup group)
        {
            var node = new TreeNode
            {
                Text = group.Name,
                Name = $"{group.Id}|Group",
                Tag = group 
            };

            return node;
        }


        private void AddChildNodes(TreeNode parentNode)
        {
            var groupId = GetGroupIdFromNode(parentNode);

            var childGroups = _context.TRelationsProperty
                .Where(r => r.IdParent == groupId)
                .Select(r => r.IdChild)
                .ToList();

            var childProperties = _context.TProperties
                .Where(p => p.GroupId == groupId)
                .ToList();

            foreach (var childGroupId in childGroups)
            {
                var childGroup = _context.TGroupsProperty.FirstOrDefault(g => g.Id == childGroupId);

                if (childGroup != null)
                {
                    var childNode = CreateNode(childGroup);
                    parentNode.Nodes.Add(childNode);
                    AddChildNodes(childNode);
                }
            }


            foreach (var property in childProperties)
            {
                var propertyNode = new TreeNode
                {
                    Text = property.Name,
                    Name = $"{property.Id}|Property",
                    Tag = property
                };

                parentNode.Nodes.Add(propertyNode);
            }
        }

   
        private long GetGroupIdFromNode(TreeNode node)
        {
            var parts = node.Name.Split('|');
            if (parts.Length == 2 && parts[1] == "Group" && long.TryParse(parts[0], out var groupId))
            {
                return groupId;
            }
            return -1; 
        }




        private void Form1_Load(object sender, EventArgs e)
        {
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _context.Database.Connection.Close();
        }

        // CRUD-операции для TGROUP
        private void CreateNewGroup(string name)
        {
    
            var maxId = _context.TGroupsProperty.Select(x => x.Id).DefaultIfEmpty(0).Max() + 1;
            var newGroup = new TGroup()
            {
                Id = maxId,
                Name = name
            };
            _context.TGroupsProperty.Add(newGroup);
            var res = _context.SaveChanges();
            if (res < 0)
                MessageBox.Show("Ошибка при создании группы");
            else
                MessageBox.Show("Сохранено");
        }
        private void CreateNewGroup(long id, string name)
        {
            var newGroup = new TGroup()
            {
                Id = id,
                Name = name
            };
            _context.TGroupsProperty.Add(newGroup);
            try
            {
                var res = _context.SaveChanges();
                if (res < 0)
                    MessageBox.Show("Ошибка при создании группы");
                else
                    MessageBox.Show("Сохранено");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        //delete group
        private void DeleteGroup(long id)
        {
            if (_context == null) return;

            var groupForDelete = _context.TGroupsProperty.SingleOrDefault(x => x.Id == id);
            if (groupForDelete == null)
            {
                MessageBox.Show($"Группа с id = {id} не найдена!");
                return;
            }
          
            _context.TGroupsProperty.Remove(groupForDelete);
            try
            {
                var res = _context.SaveChanges();
                if (res <= 0)
                    MessageBox.Show("Ошибка при удалении группы");
                else
                    MessageBox.Show("Сохранено");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        //update
        private void UpdateGroupName(long id, string name)
        {
            if (_context == null) return;
             тва Name.
            var groupForUpdate = _context.TGroupsProperty.FirstOrDefault(x => x.Id == id);
            if (groupForUpdate == null)
            {
                MessageBox.Show($"Группа для обновления с id = {id} не найдена");
                return;
            }
       
            groupForUpdate.Name = name;
            try
            {
                var res = _context.SaveChanges();
                if (res < 0)
                    MessageBox.Show("Ошибка при обновлении группы");
                else
                    MessageBox.Show("Сохранено");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        //read
        private List<TGroup> ReadGroups()
        {
            if (_context == null) return null;
            var listEntites = _context.TGroupsProperty.ToList();
            if (listEntites == null)
            {
                MessageBox.Show("Таблица TGroups пустая");
                return null;
            }
            return listEntites;
        }
        private TGroup ReadGroups(long id)
        {
            if (_context == null) return null;
            var readEntity = _context.TGroupsProperty.SingleOrDefault(x => x.Id == id);
            if (readEntity == null)
            {
                MessageBox.Show("Не надена группа с id = " + id);
                return null;
            }
            return readEntity;
        }

        // CRUD-операции для TRELATION

        //create
        private void CreateNewRelation(long parentId, long childId)
        {
            if (_context == null) return;
            var newRelation = new TRelation
            {
                IdParent = parentId,
                IdChild = childId
            };
            _context.TRelationsProperty.Add(newRelation);
            var res = _context.SaveChanges();
            if (res < 0)
                MessageBox.Show("Ошибка при создании отношения");
            else
                MessageBox.Show("Сохранено");
        }
        //delete
        private void DeleteRelation(long parentId, long childId)
        {
            if (_context == null) return;
            var relationForDelete = _context.TRelationsProperty.SingleOrDefault(x => x.IdParent == parentId && x.IdChild == childId);
            if (relationForDelete == null)
            {
                MessageBox.Show($"Связь с id_parent = {parentId} и id_child = {childId} не найдена!");
                return;
            }
            _context.TRelationsProperty.Remove(relationForDelete);
            try
            {
                var res = _context.SaveChanges();
                if (res < 0)
                    MessageBox.Show("Ошибка при удалении отношения");
                else
                    MessageBox.Show("Сохранено");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message );
            }
        }
        //update

        private void UpdateRelation(long parentId, long childId)
        {
            if (_context == null) return;
         
            var relationForUpdate = _context.TRelationsProperty.FirstOrDefault(x => x.IdParent == parentId);
            if (relationForUpdate == null)
            {
                MessageBox.Show($"Отношение для обновления с parent_id = {parentId} не найдена");
                return;
            }

            relationForUpdate.IdChild = childId;
            try
            {
                var res = _context.SaveChanges();
                if (res < 0)
                    MessageBox.Show("Ошибка при обновлении отношения");
                else
                    MessageBox.Show("Сохранено");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        //read
        private List<TRelation> ReadTrelations()
        {
            if (_context == null) return null;
            var listEntites = _context.TRelationsProperty.ToList();
            if (listEntites == null)
            {
                MessageBox.Show("Таблица TRelations пустая");
                return null;
            }
            return listEntites;
        }
        private TRelation ReadTrelations(long idChild)
        {
            if (_context == null) return null;
            var readEntity = _context.TRelationsProperty.SingleOrDefault(x => x.IdChild == idChild);
            if (readEntity == null)
            {
                MessageBox.Show("Не надено отношение с ребенком " + idChild);
                return null;
            }
            return readEntity;
        }

        //TProperty

        private void CreateNewProperty(string name, string value, long groupId)
        {
            if (_context == null) return;

            long maxId = _context.TProperties.Select(x => x.Id).DefaultIfEmpty(0).Max() + 1;

            var newProperty = new TProperty
            {
                Id = maxId,
                Name = name,
                Value = value,
                GroupId = groupId
            };

            _context.TProperties.Add(newProperty);

            try
            {
                var res = _context.SaveChanges();
                if (res > 0)
                    MessageBox.Show("Свойство успешно создано и сохранено в базе данных");
                else
                    MessageBox.Show("Ошибка при создании свойства или сохранении в базе данных");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }




        

        private List<TProperty> ReadTProperties()
        {
            if (_context == null) return null;

            var listEntities = _context.TProperties.ToList();

            if (listEntities == null || listEntities.Count == 0)
            {
                MessageBox.Show("Таблица TProperties пустая");
                return null;
            }
            else return listEntities;

        }
        private TProperty ReadTProperties(long id)
        {
            if (_context == null) return null;

            var readEntity = _context.TProperties.SingleOrDefault(x => x.Id == id);
            if (readEntity == null)
            {
                MessageBox.Show($"Свойство с id = {id} не найдено");
                return null;
            }
            return readEntity;
        }

        private void UpdateProperty(long id, string newName, string newValue, long newGroupId)
        {
            if (_context == null) return;

            var propertyForUpdate = _context.TProperties.FirstOrDefault(x => x.Id == id);

            if (propertyForUpdate == null)
            {
                MessageBox.Show($"Свойство для обновления с id = {id} не найдено");
                return;
            }

            propertyForUpdate.Name = newName;
            propertyForUpdate.Value = newValue;
            propertyForUpdate.GroupId = newGroupId;
            try
            {
                var res = _context.SaveChanges();
                if (res < 0)
                    MessageBox.Show("Ошибка при обновлении свойства");
                else
                    MessageBox.Show("Сохранено");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message );
            }
        }

        private void DeleteProperty(long id)
        {
            if (_context == null) return;

            var propertyForDelete = _context.TProperties.SingleOrDefault(x => x.Id == id);

            if (propertyForDelete == null)
            {
                MessageBox.Show($"Свойство с id = {id} не найдено!");
                return;
            }

            _context.TProperties.Remove(propertyForDelete);
            try
            {
                var res = _context.SaveChanges();
                if (res < 0)
                    MessageBox.Show("Ошибка при удалении свойства");
                else
                    MessageBox.Show("Сохранено");
            }
            catch  (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void DeletePropertyByGroup(long groupId)
        {
            if (_context == null) return;

            var propertyForDelete = _context.TProperties.SingleOrDefault(x => x.GroupId == groupId);

            if (propertyForDelete == null)
            {
          
                return;
            }

            _context.TProperties.Remove(propertyForDelete);
            try
            {
                var res = _context.SaveChanges();
                if (res < 0)
                    MessageBox.Show("Ошибка при удалении свойства");
                else
                    MessageBox.Show("Сохранено");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void RefreshTreeView()
        {
         
            treeView1.Nodes.Clear();

    
            InitRootGroup();
        }
        //save group
        private void button1_Click(object sender, EventArgs e)
        {
            string groupName = textBox1.Text;
            long idGroup = Convert.ToInt64(textBox2.Text);
            TreeNode selectedNode = treeView1.SelectedNode;

            if (selectedNode != null)
            {

                if (flagAdd)
                {
                    var getInfoParent = selectedNode.Name.Split('|');
                    long idParent = Convert.ToInt64(getInfoParent[0]);
                    CreateNewGroup(idGroup, groupName);
                    CreateNewRelation(idParent, idGroup);
                    groupBox1.Text = string.Empty;
                    HideGroupForm();
                    flagAdd = false;

                }
                if (flagUpdate)
                {
                    var getInfoParent = selectedNode.Parent.Name.Split('|');
                    UpdateGroupName(idGroup, groupName);
                    groupBox1.Text = string.Empty;
                    HideGroupForm();
                    flagUpdate = false;

                }
                RefreshTreeView();
            }


        }
        //отмена
        private void button2_Click(object sender, EventArgs e)
        {
     
            textBox1.Clear();
            textBox2.Clear();
        
            HideGroupForm();
        }
        //save property
        private void button3_Click(object sender, EventArgs e)
        {
            TreeNode selectedNode = treeView1.SelectedNode;
            if (selectedNode != null)
            {
                string propertyName = textBox3.Text;
                string propertyValue = textBox4.Text;
                long idGroup = Convert.ToInt64(textBox5.Text);
                if (selectedNode.Tag is TGroup)
                {
                    if (flagAdd)
                    {
                        CreateNewProperty(propertyName, propertyValue, idGroup);
                        flagAdd = false;

                    }
                            
                }
                else if (selectedNode.Tag is TProperty)
                {
                    var info=selectedNode.Name.Split('|');
                    long ID = Convert.ToInt64(info[0]);
                    if (flagUpdate)
                    {
                        UpdateProperty(ID, propertyName, propertyValue, idGroup);
                        flagUpdate = false;
                    }
                }
            }
            else
            
            flagUpdate = false;
            flagAdd = false;
            HidePropertyForm();
            RefreshTreeView();
        }


        //отмена свойств
        private void button4_Click(object sender, EventArgs e)
        {
     
            textBox3.Clear();
            textBox4.Clear();
            textBox5.Clear();

            HidePropertyForm();
        }
        bool flagUpdate = false;
        bool flagAdd = false;
        private void группуToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HidePropertyForm();
            ShowGroupForm();
            var maxId = _context.TGroupsProperty.Select(x => x.Id).DefaultIfEmpty(0).Max() + 1;
            textBox2.Text = maxId.ToString();
            flagAdd = true;
            flagUpdate = false;
        }

        private void HideGroupForm()
        {
            groupBox1.Visible = false;
        }
        public void ShowGroupForm()
        {
            groupBox1.Visible = true;
        }
        public void HidePropertyForm()
        {
            groupBox2.Visible = false;
        }
        private void ShowPropertyForm()
        {
            groupBox2.Visible = true;
        }

        private void свойствоToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HideGroupForm();
            TreeNode selectedNode = treeView1.SelectedNode;

            if (selectedNode != null && selectedNode.Tag is TGroup)
            {
                var parentInfo=selectedNode.Name.Split('|');
            
                ShowPropertyForm();

              
                textBox3.Text = "";
                textBox4.Text = "";
                textBox5.Text = parentInfo[0];
                flagUpdate = false;
                flagAdd = true;
            }
            else
            {
                MessageBox.Show("Выберите группу для добавления свойства.");
            }
            

        }

        private void редактироватьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode selectedNode = treeView1.SelectedNode;

            if (selectedNode != null)
            {
        
                string[] nodeInfo = selectedNode.Name.Split('|');
                string nodeType = nodeInfo[1];

        
                if (nodeType.Equals("Group"))
                {
                 
                    HidePropertyForm();
                    ShowGroupForm();


               
                    string groupId = nodeInfo[0];
                    var group = ReadGroups(Convert.ToInt64(groupId));
                    textBox1.Text = group.Name;
                    textBox2.Text = groupId;
                }
                
                else if (nodeType.Equals("Property"))
                {
                 
                    HideGroupForm();
                    ShowPropertyForm();

                    
                    long propertyId = long.Parse(nodeInfo[0]);
                    var property = ReadTProperties(propertyId);
                  
                    textBox3.Text = property.Name;
                    textBox4.Text = property.Value;
                    textBox5.Text = Convert.ToString(property.GroupId);
                }
            }
            flagAdd = false;
            flagUpdate = true;
        }

        private void удалитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
      
            TreeNode selectedNode = treeView1.SelectedNode;

            if (selectedNode != null)
            {
                
                string[] nodeInfo = selectedNode.Name.Split('|');

             
                if (selectedNode.Tag is TGroup)
                {
               

                    // Удаление записей из таблицы TPROPERTY, TRELATION, TGROUP
                    long groupId = Convert.ToInt64(nodeInfo[0]);
                    var getParentInfo = selectedNode.Parent.Name.Split('|');

                    DeleteGroup(groupId);
                    DeleteRelation(Convert.ToInt64(getParentInfo[0]), groupId);
                    DeletePropertyByGroup(groupId);
                }
         
                else if (selectedNode.Tag is TProperty)
                {
                
                    long propertyId = Convert.ToInt64(nodeInfo[0]);
                    DeleteProperty(propertyId);
                }

              

                RefreshTreeView();
            }
        }
    }



    [Table("TGROUP")]
    public class TGroup
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("Id")]
        public long Id { get; set; }

        [Column("name")]
        public string Name { get; set; }
    }

    [Table("TRELATION")]
    public class TRelation
    {
        [Key, Column(Order = 0)]
        public long IdParent { get; set; }

        [Key, Column(Order = 1)]
        public long IdChild { get; set; }
    }
    [Table("TPROPERTY")]
    public class TProperty
    {

        [Key, Column("id")]
        public long Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("value")]
        public string Value { get; set; }

        [Column("group_id")]
        public long GroupId { get; set; }

    }

    public class CoreEntityContext : DbContext
    {
        public CoreEntityContext(string connectionName) : base(connectionName)
        {
        }

        public virtual DbSet<TGroup> TGroupsProperty { get; set; }
        public virtual DbSet<TRelation> TRelationsProperty { get; set; }
        public virtual DbSet<TProperty> TProperties { get; set; }
    }
}




















