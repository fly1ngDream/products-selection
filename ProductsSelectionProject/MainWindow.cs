using System;
using System.Collections.Generic;
using System.IO;
using Gtk;
using MySql.Data.MySqlClient;

public partial class MainWindow : Gtk.Window {
    ListStore prodListStore = new ListStore(typeof(string), typeof(string), typeof(string));
    List<string> prodOrdList = new List<string>();
    List<double> priceList = new List<double>();
    List<double> priceInFileList = new List<double>();
    int productsInFileCounter = 0;
    int searchButtonClicksCounter = 0;
    bool totalPriceExists = false;

    public MainWindow () : base(Gtk.WindowType.Toplevel) {
        Build();
    }

    protected void OnDeleteEvent (object sender, DeleteEventArgs a) {
        Application.Quit();
        a.RetVal = true;
    }

    protected void searchButtonClicked (object sender, EventArgs e) {
        var dbCon = DBConnection.Instance();
        dbCon.DatabaseName = "ShopProducts";
        if (dbCon.IsConnect()) {
            string query = "";
            if (entry1.Text.Equals("")) {
                query = "SELECT ProductType, CompanyProducer, ProductPrice FROM ShopProducts ORDER BY ProductType";
            } else if (combobox1.ActiveText == "Search by product type") {
                query = $"SELECT ProductType, CompanyProducer, ProductPrice FROM ShopProducts WHERE ProductType LIKE '%{entry1.Text}%' ORDER BY ProductType";
            } else if (combobox1.ActiveText == "Search by company-producer") {
                query = $"SELECT ProductType, CompanyProducer, ProductPrice FROM ShopProducts WHERE CompanyProducer LIKE '%{entry1.Text}%' ORDER BY ProductType";
            }
            var cmd = new MySqlCommand(query, dbCon.Connection);
            var reader = cmd.ExecuteReader();

            prodListStore.Clear();
            prodOrdList.Clear();

            TreeViewColumn productTypeColumn = null, companyProducerColumn = null, productPriceColumn = null;

            if (searchButtonClicksCounter == 0) {
                treeviewPrepare(productTypeColumn, companyProducerColumn, productPriceColumn);
            }

            while (reader.Read()) {
                prodListStore.AppendValues(reader.GetString(0), reader.GetString(1), reader.GetString(2));
                prodOrdList.Add(reader.GetString(0) + " " + reader.GetString(1) + " " + reader.GetString(2));
                priceList.Add(Convert.ToDouble(reader.GetString(2)));
            }
            dbCon.Close();
            searchButtonClicksCounter++;
        }
    }

    protected void treeview1RowActivated (object o, RowActivatedArgs args) {
        if (totalPriceExists) {
            return;
        }
        int counter = Convert.ToInt32(args.Args[0].ToString());
        if (textview1.Buffer.Text != "") {
            textview1.Buffer.Text += $"\n{productsInFileCounter + 1}) {prodOrdList[counter]}";
        } else {
            textview1.Buffer.Text += $"{productsInFileCounter + 1}) {prodOrdList[counter]}";
        }
        productsInFileCounter++;

        textview1WriteTextToFile(new StreamWriter("ProductsList.txt"));
        priceInFileList.Add(priceList[counter]);
    }

    private void treeviewPrepare (TreeViewColumn productTypeColumn, TreeViewColumn companyProducerColumn, TreeViewColumn productPriceColumn) {
        productTypeColumn = new TreeViewColumn();
        productTypeColumn.Title = "Product type";
        companyProducerColumn = new TreeViewColumn();
        companyProducerColumn.Title = "Company producer";
        productPriceColumn = new TreeViewColumn();
        productPriceColumn.Title = "Product price";

        treeview1.AppendColumn(productTypeColumn);
        treeview1.AppendColumn(companyProducerColumn);
        treeview1.AppendColumn(productPriceColumn);

        treeview1.Model = prodListStore;

        CellRendererText productTypeCell = new CellRendererText();
        productTypeColumn.PackStart(productTypeCell, true);
        CellRendererText companyProducerCell = new CellRendererText();
        companyProducerColumn.PackStart(companyProducerCell, true);
        CellRendererText productPriceCell = new CellRendererText();
        productPriceColumn.PackStart(productPriceCell, true);

        productTypeColumn.AddAttribute(productTypeCell, "text", 0);
        companyProducerColumn.AddAttribute(companyProducerCell, "text", 1);
        productPriceColumn.AddAttribute(productPriceCell, "text", 2);
    }

    protected void LOSPClearMenuItemActivated (object sender, EventArgs e) {
        totalPriceExists = false;

        textview1.Buffer.Text = "";

        textview1WriteTextToFile(new StreamWriter("ProductsList.txt"));

        productsInFileCounter = 0;

        priceInFileList.Clear();
    }

    protected void LOSPDeleteLastRowMenuItemActvated (object sender, EventArgs e) {
        if (totalPriceExists) {
            return;
        }
        if (productsInFileCounter > 1 && !textview1.Buffer.Text.Contains("Загальна вартість продуктів")) {
            List<string> textviewLinesList = new List<string>(textview1.Buffer.Text.Split('\n'));
            textviewLinesList.RemoveAt(textviewLinesList.Count - 1);
            textview1.Buffer.Text = String.Join("\n", textviewLinesList.ToArray());
            productsInFileCounter--;
            priceInFileList.RemoveAt(priceInFileList.Count - 1);
        } else if (productsInFileCounter > 1 && textview1.Buffer.Text.Contains("Загальна вартість продуктів")) {
            List<string> textviewLinesList = new List<string>(textview1.Buffer.Text.Split('\n'));
            textviewLinesList.RemoveAt(textviewLinesList.Count - 1);
            textview1.Buffer.Text = String.Join("\n", textviewLinesList.ToArray());
        } else if (productsInFileCounter == 1 && !textview1.Buffer.Text.Contains("Загальна вартість продуктів")) {
            textview1.Buffer.Text = "";
            productsInFileCounter--;
            priceInFileList.RemoveAt(priceInFileList.Count - 1);
        } else if (productsInFileCounter == 0 && !textview1.Buffer.Text.Contains("Загальна вартість продуктів")) {
            textview1.Buffer.Text = "";
        }

        textview1WriteTextToFile(new StreamWriter("ProductsList.txt"));
    }

    private void textview1WriteTextToFile (StreamWriter sw) {
        sw.Write(textview1.Buffer.Text);

        sw.Close();
    }

    protected void LOSPCalculateTotalPriceMenuItemActivated (object sender, EventArgs e) {
        if (totalPriceExists) {
            return;
        }

        double sum = 0;

        priceInFileList.ForEach((el) => sum += el);

        if (textview1.Buffer.Text != "") {
            textview1.Buffer.Text += $"\nЗагальна вартість продуктів : {sum} грн.;";
        } else {
            textview1.Buffer.Text += $"Загальна вартість продуктів : {sum} грн.;";
        }

        textview1WriteTextToFile(new StreamWriter("ProductsList.txt"));

        totalPriceExists = true;
    }
}
