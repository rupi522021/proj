import { TextField } from '@material-ui/core';
import React, { Component } from 'react'

export default class VsbTable extends Component {

  constructor(props) {
    super(props);
    if (this.props.sortable == undefined) this.sortable = false;
    else this.sortable = this.props.sortable;
    this.columns = this.props.columns;
    this.columns.forEach(item => {
      if (item.sortable == undefined) item.sortable = this.sortable;
      if (item.name == undefined) item.name = "";
      if (item.render == undefined) item.render = this.defaultDataRender;
    });
    if (this.props.filter == undefined) this.filter = true;
    else this.filter = this.props.filter;
    this.state = {
      sortUpDown: "down",
      filterValue: "",
    }
    if (this.props.defaultSortBy != undefined) this.columns.forEach(item => { if (item.name == this.props.defaultSortBy) this.state.sortedBy = item.data });
    else this.state.sortedBy = this.columns[0].data;
    if (this.sortable) this.dataSort(this.props.data, this.state.sortedBy, this.state.sortUpDown == "down");
  }

  filterChange = (e) => { this.setState({ filterValue: e.target.value }); }

  columnsRender = () => {
    return <tr>
      {this.columns.map(c => (c.sortable ?
        <th data-dataname={c.data} onClick={this.onClickHeader} className={`sortable${(c.data == this.state.sortedBy ? ` ${this.state.sortUpDown}` : "")}`} key={c.data}>
          <div>{c.name}</div>
        </th>
        :
        <th key={c.data}>{c.name}</th>))}
    </tr>
  }

  filterRow = (row) => {
    let result = "hiden";
    this.columns.forEach(col => {
      if (col.data != undefined) if (String(row[col.data]).toUpperCase().indexOf(String(this.state.filterValue).toUpperCase()) != -1) result = "";
    });
    return result;
  }

  dataRender = () => {
    return this.props.data.map((item, indexRow) => <tr className={(this.filter ? (this.state.filterValue != "" ? this.filterRow(item) : "") : "")} key={indexRow}>
      {this.columns.map((col, indexCol) => <td key={`${indexRow} ${indexCol}`}>{col.render(item, col.data, indexRow)}</td>)}
    </tr>
    )
  }

  defaultDataRender = (row, data) => { return row[data]; }

  onClickHeader = (e) => {
    let newSortedBy = e.currentTarget.dataset.dataname;
    let newSortUpDown = "down";
    if (newSortedBy == this.state.sortedBy) newSortUpDown = (this.state.sortUpDown == "down" ? "up" : "down");
    this.setState({ sortedBy: newSortedBy, sortUpDown: newSortUpDown });
  }

  dataSort = (data, colName, rev) => {
    data.sort((a, b) => {
      let nameA = a[colName], nameB = b[colName];
      if (nameA == null) nameA = ""
      if (nameB == null) nameB = ""
      if (rev) {
        if (nameA > nameB) return -1;
        if (nameA < nameB) return 1;
      }
      if (nameA < nameB) { return -1; }
      if (nameA > nameB) { return 1; }
      return 0;
    });
  }

  render() {
    if (this.sortable) this.dataSort(this.props.data, this.state.sortedBy, this.state.sortUpDown == "down");
    return (
      <div className={`divTb${this.props.divClass != undefined ? ` ${this.props.divClass}` : ""}`}>
        <table className="stickyCol">
          <thead>
            <tr>
              <td colSpan="100%" className="noBorder">
                <TextField label="סינון" type="search" variant="outlined" style={{ margin: 5 }}
                  margin="dense"
                  value={this.state.filterValue}
                  onChange={this.filterChange} />
                {this.props.children}
              </td>
            </tr>
          </thead>
          <tbody>
            {this.columnsRender()}
            {this.dataRender()}
          </tbody>
        </table>
      </div>
    )
  }
}