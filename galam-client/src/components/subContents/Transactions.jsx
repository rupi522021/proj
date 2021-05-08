import React, { Component } from 'react'
import { MsgContext, apiUrl, fetchCall, GMT0, dateRender } from '../../consts/MainConst';
import FindTransactionsMod from '../Modals/FindTransactionsMod';
import VsbTable from '../VsbTable';

export default class Transactions extends Component {
  static contextType = MsgContext;
  MessageShow = (msg, isSuccessShow, delMilSec = 3000) => { this.context(msg, isSuccessShow, delMilSec = 3000) };
  constructor(props) {
    super(props);
    this.years = [];
    this.allShipTos = [];
    this.allCountries = [];
    this.allItems = [];
    this.allProductFamilies = [];
    this.data = [];
    this.columns = [
      { name: "Creation date", data: "CreationDate", render: dateRender },
      { name: "Year", data: "Year" },
      { name: "Quarter", data: "Quarter" },
      { name: "Status", data: "Status" },
      { name: "Qty", data: "Qty" },
      { name: "Item", data: "ItemNumber" },
      { name: "Item description", data: "ItemDescription" },
      { name: "Product family", data: "ProductFamilyName" },
      { name: "Customer number", data: "CustomerNumber" },
      { name: "Customer name", data: "CustomerName" },
      { name: "End customer", data: "EndCustomer" },
      { name: "Melting/Classic", data: "MeltingClassic" },
      { name: "Sale manager", data: "PersonFullName" },
      { name: "Country", data: "CountryName" },
      { name: "Region", data: "Market" },
      { name: "Created by", data: "CreatedBy" },
      { name: "Last update", data: "LastUpdateDate", render: dateRender },
      { name: "Last updated by", data: "LastUpdatedBy" },
    ];
    this.state = {
      modalToShow: "",
    };
    this.classes = {
      dateTimeStyle: {
        margin: 5,
        width: 270,
        direction: 'ltr'
      },
      yearStyle: {
        margin: 5,
        width: 100,
        direction: 'ltr'
      },
      inputStyle: {
        width: 100,
      }
    };
  }

  Find = () => {
    fetchCall('GET', `${apiUrl}/TransactionsDimensions/`, '', this.findSuccess, this.findError, this.props.logedInUser.Token, this.props.logedInUser.UserName);
  }

  findSuccess = (data) => {
    this.years = data.Years;
    this.allShipTos = data.ShipTos;
    this.allCountries = data.Countries;
    this.allItems = data.Items;
    this.allProductFamilies = data.ProductFamilies;
    this.setState({ modalToShow: "FindTransactionsMod" });
  }

  findError = (error) => { this.MessageShow(error.Message, 0); }

  findTransactions = (data) => {
    let apiParametrs = [];
    if (data.from != null) apiParametrs.push(`from=${GMT0(data.from).toISOString().split(".")[0]}`);
    if (data.to != null) apiParametrs.push(`to=${GMT0(data.to).toISOString().split(".")[0]}`);
    if (data.year != null) apiParametrs.push(`year=${data.year}`);
    if (data.quarter != null) apiParametrs.push(`quarter=${data.quarter}`);
    if (data.customerNumber != "") apiParametrs.push(`customerNumber=${data.customerNumber}`);
    if (data.shipTo != null) apiParametrs.push(`shipToName=${data.shipTo.ShipToName}`);
    if (data.item != null) apiParametrs.push(`itemNumber=${data.item.Number}`);
    if (data.productFamily != null) apiParametrs.push(`productFamilyId=${data.productFamily.Id}`);
    if (data.market != null) apiParametrs.push(`market=${data.market}`);
    if (data.country != null) apiParametrs.push(`countryId=${data.country.CountryId}`);
    let api = `${apiUrl}/Transactions/${apiParametrs.length == 0 ? "" : `?${apiParametrs.join("&")}`}`;
    fetchCall('GET', api, '', this.findTransactionsSuccess, this.findTransactionsError, this.props.logedInUser.Token, this.props.logedInUser.UserName);
  }

  findTransactionsSuccess = (data) => {
    this.data = data.Transactions;
    this.setState({ modalToShow: "" });
  }

  findTransactionsError = (error) => { this.MessageShow(error.Message, 0); }

  hideModal = () => { this.setState({ modalToShow: "" }); }

  render() {
    return (
      <div>
        <div className="divToolbar">
          <div></div>
          <div className="divToolbarImg">
            <img src="img/Find.png" onClick={this.Find} />
          </div>
          <div></div>
        </div>
        <VsbTable
          sortable={true}
          data={this.data}
          columns={this.columns}
          divClass="heightWithTwoMenu"
        />
        {this.state.modalToShow == "FindTransactionsMod" ? <FindTransactionsMod
          Years={this.years}
          AllShipTos={this.allShipTos}
          AllItems={this.allItems}
          AllCountries={this.allCountries}
          AllProductFamilies={this.allProductFamilies}
          cancelX={this.hideModal}
          findF={this.findTransactions} /> : ""}
      </div>
    )
  }
}
