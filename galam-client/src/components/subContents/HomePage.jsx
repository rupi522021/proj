import React, { Component } from 'react';
import { MsgContext, apiUrl, fetchCall } from '../../consts/MainConst';

export default class HomePage extends Component {
  static contextType = MsgContext;
  MessageShow = (msg, isSuccessShow, delMilSec = 3000) => { this.context(msg, isSuccessShow, delMilSec = 3000) };
  constructor(props) {
    super(props);
    this.zeroQ = {
      FructoseS: 0,
      FructoseN: 0,
      FructoseD: 0,
      FructoseE: 0,
      FructoseOF: 0,
      FructoseL: 0,
      Total: 0,
      FructoseSPercent: 0,
      FructoseNPercent: 0,
      FructoseDPercent: 0,
      FructoseEPercent: 0,
      FructoseOFPercent: 0,
      FructoseLPercent: 0,
    };
    this.data = {
      Q1: this.zeroQ,
      Q2: this.zeroQ,
      Q3: this.zeroQ,
      Q4: this.zeroQ,
      Total: this.zeroQ
    };
    this.state = {};
  }

  componentDidMount() { this.getForecastSum(); }

  getForecastSum = () => {
    fetchCall('GET', `${apiUrl}/ForecastSum/`, '', this.getForecastSumSuccess, this.getForecastSumError, this.props.logedInUser.Token, this.props.logedInUser.UserName);
  }

  getForecastSumSuccess = (data) => {
    let total = this.data.Total;
    for (let i = 0; i < data.ForecastSum.length; i++) {
      this.data[`Q${data.ForecastSum[i].Quarter}`] = data.ForecastSum[i];
      total.FructoseS += data.ForecastSum[i].FructoseS;
      total.FructoseN += data.ForecastSum[i].FructoseN;
      total.FructoseD += data.ForecastSum[i].FructoseD;
      total.FructoseE += data.ForecastSum[i].FructoseE;
      total.FructoseOF += data.ForecastSum[i].FructoseOF;
      total.FructoseL += data.ForecastSum[i].FructoseL;
    }
    total.FructoseS = Math.round(total.FructoseS);
    total.FructoseN = Math.round(total.FructoseN);
    total.FructoseD = Math.round(total.FructoseD);
    total.FructoseE = Math.round(total.FructoseE);
    total.FructoseOF = Math.round(total.FructoseOF);
    total.FructoseL = Math.round(total.FructoseL);
    total.Total = Math.round((total.FructoseS + total.FructoseN + total.FructoseD + total.FructoseE + total.FructoseOF + total.FructoseL));
    total.FructoseSPercent = Math.round(100 * total.FructoseS / total.Total);
    total.FructoseNPercent = Math.round(100 * total.FructoseN / total.Total);
    total.FructoseDPercent = Math.round(100 * total.FructoseD / total.Total);
    total.FructoseEPercent = Math.round(100 * total.FructoseE / total.Total);
    total.FructoseOFPercent = Math.round(100 * total.FructoseOF / total.Total);
    total.FructoseLPercent = Math.round(100 * total.FructoseL / total.Total);
    this.setState({});
  }

  getForecastSumError = (error) => { this.MessageShow(error.Message, 0); }

  render() {
    return (
      <div>
        <div className="divTb" style={{ marginTop: 40 }}>
          <table className="bigTbS">
            <thead>
              <tr>
                <th>Product family</th>
                <th>Q1</th>
                <th>Q2</th>
                <th>Q3</th>
                <th>Q4</th>
                <th>Total</th>
              </tr>
            </thead>
            <tbody className="tbodySpanTxtSize">
              {["S", "N", "D", "E", "OF"].map(fr => <tr>
                <td>Frutose {fr}</td>
                {["Q1", "Q2", "Q3", "Q4", "Total"].map(q => <td>{this.data[q][`Fructose${fr}`]} <span>({this.data[q][`Fructose${fr}Percent`]}%)</span></td>)}
              </tr>)}
              <tr className="TRow">
                <td className="tdHRowT">Total</td>
                {["Q1", "Q2", "Q3", "Q4", "Total"].map(q => <td>{this.data[q]["Total"]}</td>)}
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    )
  }
}